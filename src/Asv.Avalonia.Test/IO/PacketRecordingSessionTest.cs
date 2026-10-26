using System.Text;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.IO;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Asv.Avalonia.Test.IO;

public class PacketRecordingSessionTest
{
    private static IProtocolFactory CreateFactory() => Protocol.Create(builder => builder.Protocols.RegisterExampleProtocol());

    private static void Record(PacketRecordingSession recording, IProtocolMessage message) =>
        recording.Record("port", "endpoint", PacketDirection.Rx, DateTimeOffset.UnixEpoch, message);

    [Fact]
    public async Task Binary_PreservesWireBytesOrderAndMessageValuesBeforeMutation()
    {
        var stream = new MemoryStream();
        await using var recording = new PacketRecordingSession(CreateFactory(), PacketRecordingFormat.Binary, stream);
        var message = new ExampleMessage1 { Value1 = 1, Value3 = "first" };
        var expected = message.Serialize();
        Record(recording, message);
        message.Value1 = 2;
        message.Value3 = "second";
        expected = expected.Concat(message.Serialize()).ToArray();
        Record(recording, message);
        await recording.StopAsync();
        Record(recording, new ExampleMessage1());
        Assert.Equal(expected, stream.ToArray());
        Assert.Equal(2, recording.MessagesWritten);
        Assert.Equal(expected.Length, recording.BytesWritten);
        Assert.False(recording.IsRecording);
        Assert.Null(recording.Error);
    }

    [Fact]
    public async Task JsonLines_ContainsOneCompleteMessagePerLineAndEscapesNewlines()
    {
        var stream = new MemoryStream();
        await using var recording = new PacketRecordingSession(CreateFactory(), PacketRecordingFormat.JsonLines, stream);
        var message = new ExampleMessage1 { Value3 = "Привет\nworld\r\n\"quoted\"" };
        Record(recording, message);
        message.Value3 = "changed";
        recording.Record("second-port", "second-endpoint", PacketDirection.Tx, DateTimeOffset.UnixEpoch, message);
        await recording.StopAsync();
        var data = stream.ToArray();
        Assert.False(data.AsSpan().StartsWith(Encoding.UTF8.GetPreamble()));
        var text = Encoding.UTF8.GetString(data);
        Assert.EndsWith("\n", text);
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
        var first = JObject.Parse(lines[0]);
        var second = JObject.Parse(lines[1]);
        Assert.Equal("RX", (string?)first["direction"]);
        Assert.Equal("TX", (string?)second["direction"]);
        Assert.Equal("second-port", (string?)second["port"]);
        Assert.Equal("second-endpoint", (string?)second["endpoint"]);
        Assert.Equal("example", (string?)first["protocol"]);
        Assert.Equal("Привет\nworld\r\n\"quoted\"", (string?)first["message"]?["Value3"]);
        Assert.Equal("changed", (string?)second["message"]?["Value3"]);
        Assert.Equal(2, recording.MessagesWritten);
        Assert.Null(recording.Error);
    }

    [Fact]
    public async Task Binary_UsesOneStatefulEncoderForEachRecording()
    {
        var created = 0;
        var factory = Protocol.Create(builder =>
        {
            builder.Protocols.RegisterExampleProtocol();
            builder.Protocols.RegisterEncoder(ExampleProtocol.Info, _ =>
            {
                created++;
                return new SequenceEncoder();
            });
        });
        var stream = new MemoryStream();
        await using (var recording = new PacketRecordingSession(factory, PacketRecordingFormat.Binary, stream))
        {
            Record(recording, new ExampleMessage1());
            Record(recording, new ExampleMessage2());
        }
        Assert.Equal(new byte[] { 1, 2 }, stream.ToArray());
        var next = new MemoryStream();
        await using (var recording = new PacketRecordingSession(factory, PacketRecordingFormat.Binary, next))
        {
            Record(recording, new ExampleMessage1());
        }
        Assert.Equal(new byte[] { 1 }, next.ToArray());
        Assert.Equal(2, created);
    }

    [Fact]
    public async Task DiskFailure_StopsRecordingAndExposesErrorWithoutThrowingIntoCapture()
    {
        var stream = new FailingStream();
        await using var recording = new PacketRecordingSession(CreateFactory(), PacketRecordingFormat.Binary, stream);
        Record(recording, new ExampleMessage1());
        await recording.StopAsync();
        Assert.IsType<IOException>(recording.Error);
        Assert.False(recording.IsRecording);
        Assert.Equal(0, recording.MessagesWritten);
        Assert.True(stream.Closed);
    }

    [Fact]
    public async Task SlowDisk_BoundedQueueStopsInsteadOfSilentlyDroppingMessages()
    {
        var stream = new BlockedStream();
        await using var recording = new PacketRecordingSession(CreateFactory(), PacketRecordingFormat.Binary, stream);
        Record(recording, new ExampleMessage1());
        await stream.Started.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        try
        {
            for (var i = 0; i < 4097; i++)
            {
                Record(recording, new ExampleMessage1());
            }
            Assert.IsType<IOException>(recording.Error);
            Assert.False(recording.IsRecording);
        }
        finally { stream.Release.TrySetResult(); }
        await recording.StopAsync();
        Assert.Equal(4097, recording.MessagesWritten);
    }

    [Fact]
    public async Task ConcurrentMessages_ProduceCompleteJsonLinesWithoutInterleaving()
    {
        var stream = new MemoryStream();
        await using var recording = new PacketRecordingSession(CreateFactory(), PacketRecordingFormat.JsonLines, stream);
        Parallel.For(0, 100, value => Record(recording, new ExampleMessage1 { Value1 = value }));
        await recording.StopAsync();
        var lines = Encoding.UTF8.GetString(stream.ToArray()).Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(100, lines.Length);
        Assert.Equal(Enumerable.Range(0, 100), lines.Select(line => (int)JObject.Parse(line)["message"]!["Value1"]!).Order());
        Assert.Null(recording.Error);
    }

    [Fact]
    public async Task SerializationFailure_StopsExplicitlyAndReleasesTheStream()
    {
        var factory = Protocol.Create(builder =>
        {
            builder.Protocols.RegisterExampleProtocol();
            builder.Protocols.RegisterEncoder(ExampleProtocol.Info, _ => new FailingEncoder());
        });
        var stream = new MemoryStream();
        await using var recording = new PacketRecordingSession(factory, PacketRecordingFormat.Binary, stream);
        Record(recording, new ExampleMessage1());
        await recording.StopAsync();
        Assert.IsType<InvalidDataException>(recording.Error);
        Assert.Empty(stream.ToArray());
        Assert.False(recording.IsRecording);
        Assert.False(stream.CanWrite);
    }

    private sealed class FailingEncoder : IProtocolEncoder
    {
        public int GetMaxByteSize(IProtocolMessage message) => 1;
        public int Encode(IProtocolMessage message, Span<byte> destination) => throw new InvalidDataException("cannot encode");
    }

    private sealed class SequenceEncoder : IProtocolEncoder
    {
        private byte _sequence;
        public int GetMaxByteSize(IProtocolMessage message) => 4;
        public int Encode(IProtocolMessage message, Span<byte> destination)
        {
            destination[0] = ++_sequence;
            return 1;
        }
    }

    private sealed class FailingStream : MemoryStream
    {
        public bool Closed { get; private set; }
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) =>
            ValueTask.FromException(new IOException("disk full"));
        protected override void Dispose(bool disposing) { Closed = true; base.Dispose(disposing); }
    }

    private sealed class BlockedStream : MemoryStream
    {
        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            Started.TrySetResult();
            await Release.Task.ConfigureAwait(false);
        }
    }
}

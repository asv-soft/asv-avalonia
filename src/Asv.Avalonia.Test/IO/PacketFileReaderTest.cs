using System.Diagnostics.CodeAnalysis;
using Asv.Avalonia.IO;
using Asv.Common;
using Asv.IO;
using Asv.IO.Device;
using Material.Icons;
using Xunit;

namespace Asv.Avalonia.Test.IO;

public class PacketFileReaderTest
{
    [Fact]
    public async Task Read_PreservesOrderOffsetsDevicesAndVisitorValuesAcrossBlocks()
    {
        var first = new ExampleMessage1 { SenderId = 1, Value1 = 42, Value3 = "first" }.Serialize();
        var second = new ExampleMessage1 { SenderId = 2, Value1 = 84, Value3 = "second" }.Serialize();
        using var file = new TestFile(new byte[8191].Concat(first).Concat(second).ToArray());
        var manager = new TestManager();
        var reader = new PacketFileReader(manager, [new DefaultPacketFieldSource()]);

        var result = await reader.ReadAsync(file.Path, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(new long[] { 1, 2 }, result.State.Packets.Select(x => x.Sequence));
        Assert.Equal(new long?[] { 8191 + first.Length, 8191 + first.Length + second.Length }, result.State.Packets.Select(x => x.FileEndOffset));
        Assert.Equal(8191 + first.Length + second.Length, result.Bytes);
        Assert.Equal(2, manager.Identifications);
        Assert.NotEqual(result.State.Packets[0].Key, result.State.Packets[1].Key);
        Assert.Equal("first", Assert.Single(result.State.Packets[0].Fields).Children.Single(x => x.Name == "Value3").Value);
        Assert.Equal("second", Assert.Single(result.State.Packets[1].Fields).Children.Single(x => x.Name == "Value3").Value);
        Assert.All(result.State.Packets, x => { Assert.Empty(x.Time); Assert.Empty(x.Direction); });
        Assert.Equal(0, result.State.Errors);
        var value = Assert.Single(result.State.Packets[0].Fields).Children.Single(x => x.Name == "Value1");
        Assert.Equal("Value1", value.Definition?.Name);
        Assert.IsType<Int32Type>(value.DataType);
    }

    [Fact]
    public async Task Read_CorruptPacket_ReportsErrorAndRecovers()
    {
        var bad = new ExampleMessage1 { Value1 = 1 }.Serialize();
        bad[^1] ^= 0xFF;
        var good = new ExampleMessage1 { Value1 = 77 }.Serialize();
        using var file = new TestFile(bad.Concat(good).ToArray());
        var reader = new PacketFileReader(new TestManager(), [new DefaultPacketFieldSource()]);

        var result = await reader.ReadAsync(file.Path, cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(result.State.Errors > 0);
        var packet = Assert.Single(result.State.Packets);
        Assert.Equal(bad.Length + good.Length, packet.FileEndOffset);
        Assert.Equal("77", Assert.Single(packet.Fields).Children.Single(x => x.Name == "Value1").Value);
    }

    [Fact]
    public async Task Read_EmptyFile_ReturnsEmptyCapture()
    {
        using var file = new TestFile([]);
        var result = await new PacketFileReader(new TestManager(), [new DefaultPacketFieldSource()]).ReadAsync(file.Path, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Empty(result.State.Packets);
        Assert.Equal(0, result.Bytes);
        Assert.Equal(0, result.State.Errors);
    }

    [Fact]
    public async Task Read_Canceled_ReleasesFileAndAllowsReadingAgain()
    {
        using var file = new TestFile(new ExampleMessage1().Serialize());
        var reader = new PacketFileReader(new TestManager(), [new DefaultPacketFieldSource()]);
        using var cancel = new CancellationTokenSource();
        cancel.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => reader.ReadAsync(file.Path, cancellationToken: cancel.Token));
        Assert.Single((await reader.ReadAsync(file.Path, cancellationToken: TestContext.Current.CancellationToken)).State.Packets);
        using var exclusive = File.Open(file.Path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        Assert.True(exclusive.CanWrite);
    }

    [Fact]
    public async Task Read_UnknownDevice_RetainsMessage()
    {
        using var file = new TestFile(new ExampleMessage1().Serialize());
        var result = await new PacketFileReader(new TestManager { Identify = false }, [new DefaultPacketFieldSource()]).ReadAsync(file.Path, cancellationToken: TestContext.Current.CancellationToken);
        Assert.Null(Assert.Single(result.State.Packets).Key.DeviceId);
        Assert.NotEmpty(result.State.Packets[0].Fields);
    }

    private sealed class TestFile(byte[] bytes) : IDisposable
    {
        public string Path { get; } = Create(bytes);
        private static string Create(byte[] bytes)
        {
            var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"asv-inspector-{Guid.NewGuid():N}.bin");
            File.WriteAllBytes(path, bytes);
            return path;
        }
        public void Dispose() => File.Delete(Path);
    }

    private sealed class TestManager : IDeviceManager
    {
        public IProtocolFactory ProtocolFactory { get; } = Protocol.Create(builder => builder.Protocols.RegisterExampleProtocol());
        public bool Identify { get; init; } = true;
        public int Identifications { get; private set; }
        public bool TryIdentify(IProtocolMessage message, [NotNullWhen(true)] out DeviceId? deviceId)
        {
            Identifications++;
            deviceId = Identify ? new ExampleDeviceId("example", ((ExampleMessageBase)message).SenderId) : null;
            return deviceId != null;
        }
        public MaterialIconKind? GetIcon(DeviceId id) => MaterialIconKind.Navigation;
        public AsvColorKind GetDeviceColor(DeviceId id) => AsvColorKind.None;
        public IProtocolRouter Router => throw new NotSupportedException();
        public IDeviceExplorer Explorer => throw new NotSupportedException();
    }
}

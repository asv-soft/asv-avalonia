using System.Text;
using System.Threading.Channels;
using Asv.IO;

namespace Asv.Avalonia.IO;

public enum PacketRecordingFormat
{
    Binary,
    JsonLines,
}

/// <summary>Serializes live messages before they can be reused and writes them in capture order.</summary>
public sealed class PacketRecordingSession : IAsyncDisposable
{
    private const long MaximumQueuedBytes = 32 * 1024 * 1024;
    private readonly object _gate = new();
    private readonly IProtocolFactory _factory;
    private readonly PacketRecordingFormat _format;
    private readonly Stream _stream;
    private readonly Channel<byte[]> _queue = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(4096)
    {
        SingleReader = true,
        SingleWriter = true,
        FullMode = BoundedChannelFullMode.Wait,
        AllowSynchronousContinuations = false,
    });
    private readonly Dictionary<string, IProtocolEncoder> _encoders = new();
    private readonly Task _writer;
    private bool _accepting = true;
    private Exception? _error;
    private long _queuedBytes;
    private long _messagesWritten;
    private long _bytesWritten;

    public PacketRecordingSession(IProtocolFactory factory, PacketRecordingFormat format, Stream stream)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(stream);
        if (!Enum.IsDefined(format))
        {
            throw new ArgumentOutOfRangeException(nameof(format));
        }
        _factory = factory;
        _format = format;
        _stream = stream;
        _writer = Task.Run(WriteAsync);
    }

    public bool IsRecording { get { lock (_gate) { return _accepting; } } }
    public Exception? Error { get { lock (_gate) { return _error; } } }
    public long MessagesWritten => Interlocked.Read(ref _messagesWritten);
    public long BytesWritten => Interlocked.Read(ref _bytesWritten);

    public void Record(string portId, string endpointId, PacketDirection direction, DateTimeOffset timestamp, IProtocolMessage message)
    {
        lock (_gate)
        {
            if (!_accepting)
            {
                return;
            }
            try
            {
                byte[] data;
                if (_format == PacketRecordingFormat.Binary)
                {
                    if (!_encoders.TryGetValue(message.Protocol.Id, out var encoder))
                    {
                        encoder = _factory is IProtocolContext context
                            ? context.CreateEncoder(message.Protocol.Id) : DefaultProtocolEncoder.Instance;
                        _encoders.Add(message.Protocol.Id, encoder);
                    }
                    data = message.EncodeToArray(encoder);
                }
                else
                {
                    var json = PacketJsonLines.Serialize(timestamp, direction, portId, endpointId, message);
                    data = Encoding.UTF8.GetBytes(json + "\n");
                }
                if (data.LongLength > MaximumQueuedBytes - _queuedBytes || !_queue.Writer.TryWrite(data))
                {
                    Fail(new IOException(RecordingStrings.BufferFull));
                    return;
                }
                _queuedBytes += data.LongLength;
            }
            catch (Exception error)
            {
                Fail(error);
            }
        }
    }

    public async ValueTask StopAsync()
    {
        lock (_gate)
        {
            _accepting = false;
            _queue.Writer.TryComplete();
        }
        await _writer.ConfigureAwait(false);
    }

    public ValueTask DisposeAsync() => StopAsync();

    private void Fail(Exception error)
    {
        _error ??= error;
        _accepting = false;
        _queue.Writer.TryComplete();
    }

    private async Task WriteAsync()
    {
        try
        {
            await foreach (var data in _queue.Reader.ReadAllAsync().ConfigureAwait(false))
            {
                await _stream.WriteAsync(data).ConfigureAwait(false);
                lock (_gate) { _queuedBytes -= data.LongLength; }
                Interlocked.Increment(ref _messagesWritten);
                Interlocked.Add(ref _bytesWritten, data.LongLength);
            }
            await _stream.FlushAsync().ConfigureAwait(false);
        }
        catch (Exception error)
        {
            lock (_gate) { Fail(error); }
        }
        finally
        {
            try { await _stream.DisposeAsync().ConfigureAwait(false); }
            catch (Exception error) { lock (_gate) { Fail(error); } }

            // Release queued frames after a failed disk write.
            while (_queue.Reader.TryRead(out _)) { }
            lock (_gate) { _queuedBytes = 0; }
        }
    }
}

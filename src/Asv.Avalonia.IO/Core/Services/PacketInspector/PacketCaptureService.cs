using Asv.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using R3;

namespace Asv.Avalonia.IO;

public sealed class PacketInspectorOptions
{
    public int MaximumPackets { get; set; } = 10_000;
    public long MaximumBytes { get; set; } = 32 * 1024 * 1024;
}

public sealed record PacketSource
{
    public PacketSource(
        string portId,
        string portName,
        string endpointId,
        string? portDescription = null,
        string? endpointName = null,
        string? endpointDescription = null
    )
    {
        PortId = portId;
        PortName = portName;
        EndpointId = endpointId;
        PortDescription = portDescription ?? portId;
        EndpointName = endpointName ?? endpointId;
        EndpointDescription = endpointDescription ?? endpointId;
    }

    public string PortId { get; init; }
    public string PortName { get; init; }
    public string EndpointId { get; init; }
    public string PortDescription { get; init; }
    public string EndpointName { get; init; }
    public string EndpointDescription { get; init; }

    internal static PacketSource FromPacket(PacketSnapshot packet) => new(
        packet.Key.PortId,
        packet.PortName,
        packet.Endpoint,
        packet.PortDescription,
        packet.EndpointName,
        packet.EndpointDescription);
}

public sealed record PacketPort
{
    public PacketPort(string id, string displayName, string description)
    {
        Id = id;
        DisplayName = displayName;
        Description = description;
    }

    public string Id { get; init; }
    public string DisplayName { get; init; }
    public string Description { get; init; }
}

public sealed record PacketCaptureState
{
    public PacketCaptureState(
        IReadOnlyList<PacketSnapshot> packets,
        IReadOnlyList<PacketSource> sources,
        long evicted,
        long errors
    )
    {
        Packets = packets;
        Sources = sources;
        Evicted = evicted;
        Errors = errors;
    }

    public IReadOnlyList<PacketSnapshot> Packets { get; init; }
    public IReadOnlyList<PacketSource> Sources { get; init; }
    public long Evicted { get; init; }
    public long Errors { get; init; }
    public IReadOnlyList<PacketPort> Ports { get; init; } = [];
    public IReadOnlyList<ProtocolInfo> Protocols { get; init; } = [];
}

/// <summary>Owns transport subscriptions and a bounded, immutable packet history.</summary>
public sealed class PacketCaptureService : IDisposable, IAsyncDisposable
{
    private readonly object _gate = new();
    private readonly IDeviceManager _manager;
    private readonly TimeProvider _time;
    private readonly ILogger<PacketCaptureService> _logger;
    private readonly IPacketFieldSource[] _fieldSources;
    private readonly PacketInspectorOptions _options;
    private readonly Queue<PacketSnapshot> _packets = new();
    private readonly Dictionary<IProtocolPort, CompositeDisposable> _ports = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<IProtocolEndpoint, (IProtocolPort Port, CompositeDisposable Subscriptions)> _endpoints = new(ReferenceEqualityComparer.Instance);
    private readonly CompositeDisposable _subscriptions = new();
    private long _sequence;
    private long _bytes;
    private long _evicted;
    private long _errors;
    private bool _disposed;
    private PacketRecordingSession? _recording;
    private Task _recordingsCompletion = Task.CompletedTask;

    public PacketCaptureService(
        IDeviceManager manager,
        TimeProvider time,
        IEnumerable<IPacketFieldSource> fieldSources,
        IOptions<PacketInspectorOptions> options,
        ILogger<PacketCaptureService> logger
    )
    {
        _manager = manager;
        _time = time;
        _logger = logger;
        _fieldSources = fieldSources.OrderBy(x => x is DefaultPacketFieldSource ? 1 : 0).ToArray();
        _options = options.Value;
        ArgumentOutOfRangeException.ThrowIfLessThan(_options.MaximumPackets, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(_options.MaximumBytes, 1024);
        manager.Router.PortAdded.Subscribe(AddPort).AddTo(_subscriptions);
        manager.Router.PortRemoved.Subscribe(RemovePort).AddTo(_subscriptions);
        foreach (var port in manager.Router.Ports)
        {
            AddPort(port);
        }
    }

    public void AttachRecording(PacketRecordingSession recording)
    {
        lock (_gate)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_recording != null)
            {
                throw new InvalidOperationException("A traffic recording is already attached.");
            }
            _recording = recording;
        }
    }

    public void DetachRecording(PacketRecordingSession recording)
    {
        lock (_gate)
        {
            if (ReferenceEquals(_recording, recording))
            {
                _recording = null;
                var completion = recording.StopAsync().AsTask();
                _recordingsCompletion = _recordingsCompletion.IsCompleted
                    ? completion : Task.WhenAll(_recordingsCompletion, completion);
            }
        }
    }

    private void AddPort(IProtocolPort port)
    {
        lock (_gate)
        {
            if (_disposed || port.IsDisposed || _ports.ContainsKey(port))
            {
                return;
            }
            var subscriptions = new CompositeDisposable();
            _ports.Add(port, subscriptions);
            port.EndpointAdded.Subscribe(endpoint => AddEndpoint(port, endpoint)).AddTo(subscriptions);
            port.EndpointRemoved.Subscribe(RemoveEndpoint).AddTo(subscriptions);
            foreach (var endpoint in port.Endpoints)
            {
                AddEndpoint(port, endpoint);
            }
        }
    }

    private void RemovePort(IProtocolPort port)
    {
        lock (_gate)
        {
            if (_ports.Remove(port, out var subscriptions))
            {
                subscriptions.Dispose();
            }
            foreach (var endpoint in _endpoints.Where(x => ReferenceEquals(x.Value.Port, port)).Select(x => x.Key).ToArray())
            {
                RemoveEndpoint(endpoint);
            }
        }
    }

    private void AddEndpoint(IProtocolPort port, IProtocolEndpoint endpoint)
    {
        lock (_gate)
        {
            if (_disposed || !_ports.ContainsKey(port) || endpoint.IsDisposed || _endpoints.ContainsKey(endpoint))
            {
                return;
            }
            var subscriptions = new CompositeDisposable();
            _endpoints.Add(endpoint, (port, subscriptions));
            endpoint.OnRxMessage.Subscribe(message => Capture(port, endpoint, PacketDirection.Rx, message)).AddTo(subscriptions);
            endpoint.OnTxMessage.Subscribe(message => Capture(port, endpoint, PacketDirection.Tx, message)).AddTo(subscriptions);
            endpoint.OnRxError.Subscribe(RecordError).AddTo(subscriptions);
            endpoint.OnTxError.Subscribe(RecordError).AddTo(subscriptions);
        }
    }

    private void RemoveEndpoint(IProtocolEndpoint endpoint)
    {
        lock (_gate)
        {
            if (_endpoints.Remove(endpoint, out var entry))
            {
                entry.Subscriptions.Dispose();
            }
        }
    }

    private void RecordError(Exception exception)
    {
        lock (_gate)
        {
            if (!_disposed)
            {
                _errors++;
                _logger.LogDebug(exception, "Packet inspector transport error");
            }
        }
    }

    private void Capture(IProtocolPort port, IProtocolEndpoint endpoint, PacketDirection direction, IProtocolMessage message)
    {
        lock (_gate)
        {
            if (_disposed || !_endpoints.ContainsKey(endpoint))
            {
                return;
            }
            var timestamp = _time.GetUtcNow();
            var monotonic = _time.GetTimestamp();
            _recording?.Record(port.Id, endpoint.Id, direction, timestamp, message);
            try
            {
                DeviceId? deviceId = null;
                string? error = null;
                try
                {
                    if (!_manager.TryIdentify(message, out deviceId))
                    {
                        deviceId = null;
                    }
                }
                catch (Exception ex)
                {
                    deviceId = null;
                    error = ex.Message;
                    _errors++;
                }
                DeviceAddress? sourceAddress = null;
                try
                {
                    if (_manager.TryGetSourceAddress(message, out var address)
                        && !string.IsNullOrWhiteSpace(address.Family) && !string.IsNullOrWhiteSpace(address.Value))
                    {
                        sourceAddress = address;
                    }
                }
                catch (Exception addressError)
                {
                    error ??= addressError.Message;
                    _errors++;
                }
                var fields = new List<PacketFieldSnapshot>();
                try
                {
                    foreach (var source in _fieldSources)
                    {
                        foreach (var section in source.GetSections(message))
                        {
                            var visitor = new PacketFieldsVisitor();
                            section.Value.Accept(visitor);
                            fields.Add(new(section.Key, string.Empty, string.Empty, null, null, visitor.Fields));
                        }
                        if (fields.Count > 0)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    _errors++;
                }
                var key = new PacketGroupKey(port.Id, endpoint.Id, direction, message.Protocol.Id, message.GetIdAsString(), deviceId)
                {
                    SourceAddress = sourceAddress,
                };
                var snapshot = new PacketSnapshot(
                    ++_sequence,
                    timestamp,
                    monotonic,
                    key,
                    port.DisplayName,
                    message.Protocol.Name,
                    message.Name,
                    fields.ToArray(),
                    error
                )
                {
                    PortDescription = port.Description,
                    EndpointName = endpoint.DisplayName,
                    EndpointDescription = endpoint.Description,
                };
                var size = snapshot.EstimatedBytes;
                while (_packets.Count > 0 && (_packets.Count >= _options.MaximumPackets || _bytes + size > _options.MaximumBytes))
                {
                    _bytes -= _packets.Dequeue().EstimatedBytes;
                    _evicted++;
                }
                if (size > _options.MaximumBytes)
                {
                    _evicted++;
                    return;
                }
                _packets.Enqueue(snapshot);
                _bytes += size;
            }
            catch (Exception ex)
            {
                _errors++;
                _logger.LogWarning(ex, "Unable to inspect protocol message");
            }
        }
    }

    public PacketCaptureState Read()
    {
        lock (_gate)
        {
            var sources = _endpoints.Select(
                x => new PacketSource(
                    x.Value.Port.Id,
                    x.Value.Port.DisplayName,
                    x.Key.Id,
                    x.Value.Port.Description,
                    x.Key.DisplayName,
                    x.Key.Description
                )
            ).ToArray();
            return new(_packets.ToArray(), sources, _evicted, _errors)
            {
                Ports = _ports.Keys.Select(x => new PacketPort(x.Id, x.DisplayName, x.Description)).ToArray(),
                Protocols = _manager.Router.AvailableProtocols,
            };
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            _packets.Clear();
            _bytes = 0;
            _evicted = 0;
            _errors = 0;
        }
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            if (_recording != null)
            {
                DetachRecording(_recording);
            }
            _subscriptions.Dispose();
            foreach (var entry in _endpoints.Values)
            {
                entry.Subscriptions.Dispose();
            }
            foreach (var subscription in _ports.Values)
            {
                subscription.Dispose();
            }
            _endpoints.Clear();
            _ports.Clear();
            _packets.Clear();
        }
    }

    public async ValueTask DisposeAsync()
    {
        Dispose();
        Task completion;
        lock (_gate) { completion = _recordingsCompletion; }
        await completion.ConfigureAwait(false);
    }
}

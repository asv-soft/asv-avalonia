using System.Buffers;
using Asv.IO;
using R3;

namespace Asv.Avalonia.IO;

public sealed record PacketFileResult(string path, long bytes, PacketCaptureState state)
{
    public string Path { get; } = path;
    public long Bytes { get; } = bytes;
    public PacketCaptureState State { get; } = state;
}

/// <summary>Reads a file using the application's registered protocol parsers.</summary>
public sealed class PacketFileReader(IDeviceManager manager, IEnumerable<IPacketFieldSource> fieldSources)
{
    private const long MaximumSnapshotBytes = 128 * 1024 * 1024;
    private const int MaximumPackets = 100_000;
    private readonly IPacketFieldSource[] _sources = fieldSources.OrderBy(x => x is DefaultPacketFieldSource ? 1 : 0).ToArray();

    public async Task<PacketFileResult> ReadAsync(string path, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.GetFullPath(path);
        cancellationToken.ThrowIfCancellationRequested();
        await using var stream = new FileStream(fullPath, new FileStreamOptions
        {
            Mode = FileMode.Open,
            Access = FileAccess.Read,
            Share = FileShare.Read,
            Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
        });
        var packets = new List<PacketSnapshot>();
        var parsers = new List<IProtocolParser>();
        using var subscriptions = new CompositeDisposable();
        long position = 0, errors = 0, snapshotBytes = 0;
        Exception? failure = null;
        var buffer = ArrayPool<byte>.Shared.Rent(8192);
        try
        {
            foreach (var protocol in manager.ProtocolFactory.AvailableProtocols)
            {
                var parser = manager.ProtocolFactory.CreateParser(protocol.Id);
                parsers.Add(parser);
                parser.OnError.Subscribe(_ => errors++, _ => errors++, _ => { }).AddTo(subscriptions);
                parser.OnMessage.Subscribe(message =>
                {
                    if (failure != null)
                    {
                        return;
                    }
                    try
                    {
                        DeviceId? device = null;
                        string? inspectionError = null;
                        try
                        {
                            if (!manager.TryIdentify(message, out device))
                            {
                                device = null;
                            }
                        }
                        catch (Exception error)
                        {
                            device = null;
                            inspectionError = error.Message;
                            errors++;
                        }
                        DeviceAddress? sourceAddress = null;
                        try
                        {
                            if (manager.TryGetSourceAddress(message, out var address)
                                && !string.IsNullOrWhiteSpace(address.Family) && !string.IsNullOrWhiteSpace(address.Value))
                            {
                                sourceAddress = address;
                            }
                        }
                        catch (Exception addressError)
                        {
                            inspectionError ??= addressError.Message;
                            errors++;
                        }
                        var fields = new List<PacketFieldSnapshot>();
                        try
                        {
                            foreach (var source in _sources)
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
                        catch (Exception error)
                        {
                            inspectionError = error.Message;
                            errors++;
                        }
                        var snapshot = new PacketSnapshot(
                            packets.Count + 1L,
                            default,
                            0,
                            new(fullPath, fullPath, PacketDirection.Rx, message.Protocol.Id, message.GetIdAsString(), device)
                            {
                                SourceAddress = sourceAddress,
                            },
                            Path.GetFileName(fullPath),
                            message.Protocol.Name,
                            message.Name,
                            fields.ToArray(),
                            inspectionError)
                        {
                            FileEndOffset = position,
                            EndpointName = Path.GetFileName(fullPath),
                            EndpointDescription = fullPath,
                        };
                        snapshotBytes += snapshot.EstimatedBytes;
                        if (packets.Count >= MaximumPackets || snapshotBytes > MaximumSnapshotBytes)
                        {
                            failure = new InvalidDataException(RS.FileInspector_Limit);
                            return;
                        }
                        packets.Add(snapshot);
                    }
                    catch (Exception error) { failure = error; }
                }).AddTo(subscriptions);
            }
            if (parsers.Count == 0)
            {
                throw new InvalidOperationException(RS.FileInspector_NoParsers);
            }
            while (true)
            {
                var count = await stream.ReadAsync(buffer.AsMemory(0, 8192), cancellationToken).ConfigureAwait(false);
                if (count == 0)
                {
                    break;
                }
                for (var index = 0; index < count; index++)
                {
                    if ((index & 255) == 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    position++;
                    foreach (var parser in parsers)
                    {
                        try
                        {
                            parser.Push(buffer[index]);
                        }
                        catch (Exception error) when (error is not OutOfMemoryException)
                        {
                            errors++;
                            parser.Reset();
                        }
                        if (failure != null)
                        {
                            throw failure;
                        }
                    }
                }
                progress?.Report(stream.Length == 0 ? 100 : 100.0 * position / stream.Length);
            }
            cancellationToken.ThrowIfCancellationRequested();
            return new(fullPath, position, new(packets.ToArray(),
                [new(fullPath, Path.GetFileName(fullPath), fullPath)], 0, errors));
        }
        finally
        {
            subscriptions.Dispose();
            foreach (var parser in parsers)
            {
                parser.Dispose();
            }
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}

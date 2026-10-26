using System.Buffers;
using System.Text;
using Asv.IO;
using R3;

namespace Asv.Avalonia.IO;

/// <summary>Reports the number of exported messages and protocol parsing errors.</summary>
public sealed record PacketFileExportResult(long messages, long errors)
{
    public long Messages { get; } = messages;
    public long Errors { get; } = errors;
}

/// <summary>Streams all recognized messages from a binary file to JSON Lines.</summary>
public sealed class PacketFileExporter(IProtocolFactory factory)
{
    /// <summary>Exports in file order, replacing the destination only after successful completion.</summary>
    public Task<PacketFileExportResult> ExportJsonLinesAsync(string source, string destination,
        IProgress<double>? progress = null, CancellationToken cancellationToken = default) =>
        Task.Run(() => ExportCoreAsync(source, destination, progress, cancellationToken), cancellationToken);

    private async Task<PacketFileExportResult> ExportCoreAsync(string source, string destination,
        IProgress<double>? progress, CancellationToken cancellationToken)
    {
        source = Path.GetFullPath(source);
        destination = Path.GetFullPath(destination);
        if (string.Equals(source, destination, StringComparison.OrdinalIgnoreCase))
        {
            throw new IOException(FileExportStrings.SameFile);
        }
        var temporary = Path.Combine(Path.GetDirectoryName(destination)!, $".{Path.GetFileName(destination)}.{Guid.NewGuid():N}.tmp");
        var parsers = new List<IProtocolParser>();
        using var subscriptions = new CompositeDisposable();
        var buffer = ArrayPool<byte>.Shared.Rent(8192);
        long messages = 0, errors = 0;
        Exception? failure = null;
        try
        {
            await using (var input = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, FileOptions.Asynchronous | FileOptions.SequentialScan))
            await using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 8192, FileOptions.Asynchronous | FileOptions.SequentialScan))
            await using (var writer = new StreamWriter(output, new UTF8Encoding(false), 8192, leaveOpen: true))
            {
                foreach (var protocol in factory.AvailableProtocols)
                {
                    var parser = factory.CreateParser(protocol.Id);
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
                            writer.WriteLine(PacketJsonLines.Serialize(null, null, source, source, message));
                            messages++;
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
                    var count = await input.ReadAsync(buffer.AsMemory(0, 8192), cancellationToken).ConfigureAwait(false);
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
                        foreach (var parser in parsers)
                        {
                            try { parser.Push(buffer[index]); }
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
                    progress?.Report(input.Length == 0 ? 100 : 100.0 * input.Position / input.Length);
                }
                await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporary, destination, overwrite: true);
            return new(messages, errors);
        }
        finally
        {
            subscriptions.Dispose();
            foreach (var parser in parsers)
            {
                parser.Dispose();
            }
            ArrayPool<byte>.Shared.Return(buffer);
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }
}

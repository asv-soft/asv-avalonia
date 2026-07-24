using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using DotNext.Collections.Generic;
using Newtonsoft.Json;

namespace Asv.Avalonia;

public class LogReaderOptions
{
    public const string Section = "LogViewer";
    public string Folder { get; set; } = "logs";
}

/// <summary>
/// Default <see cref="ILogReaderService"/> implementation that reads the rolling JSON log files
/// written by the file logging provider to the application log folder.
/// </summary>
public class LogReaderService : ILogReaderService
{
    private readonly string _logsFolder;

    public LogReaderService(LogReaderOptions options, IAppPath appPath)
    {
        ArgumentNullException.ThrowIfNull(appPath);

        _logsFolder = appPath.GetAppPathFolder(options.Folder);
        ArgumentNullException.ThrowIfNull(_logsFolder);
    }

    /// <inheritdoc />
    [RequiresUnreferencedCode(
        "Uses Newtonsoft.Json reflection-based serialization, which is not trim safe."
    )]
    public async IAsyncEnumerable<LogMessage> LoadItemsFromLogFile(
        [EnumeratorCancellation] CancellationToken cancel = default
    )
    {
        var serializer = JsonSerializer.CreateDefault();
        var result = new Stack<LogMessage>();
        foreach (
            var logFilePath in Directory.EnumerateFiles(_logsFolder, "*.logs").OrderDescending()
        )
        {
            if (cancel.IsCancellationRequested)
            {
                break;
            }

            await using var fs = new FileStream(
                logFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                64 * 1024, // 64 Kb
                FileOptions.Asynchronous | FileOptions.SequentialScan
            );
            using var sr = new StreamReader(fs, Encoding.UTF8, true, 64 * 1024, true);

            await using var rdr = new JsonTextReader(sr) { SupportMultipleContent = true };

            while (
                !cancel.IsCancellationRequested && await rdr.ReadAsync(cancel).ConfigureAwait(false)
            )
            {
                if (rdr.TokenType != JsonToken.StartObject)
                {
                    continue;
                }

                var item = serializer.Deserialize<LogMessage>(rdr);
                if (item is not null)
                {
                    result.Push(item);
                }
            }
        }

        foreach (var logMessage in result)
        {
            yield return logMessage;
        }
    }
}

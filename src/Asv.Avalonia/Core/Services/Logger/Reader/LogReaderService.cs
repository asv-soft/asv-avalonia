using System.Runtime.CompilerServices;
using System.Text;
using DotNext.Collections.Generic;
using Newtonsoft.Json;

namespace Asv.Avalonia;

public class LogReaderOptions
{
    public const string Section = "LogViewer";
    public int RollingSizeKb { get; set; } = 50;
    public string Folder { get; set; } = "logs";
}

public class LogReaderService : ILogReaderService
{
    private static readonly JsonSerializer Serializer = new();

    private readonly string _logsFolder;

    public LogReaderService(LogReaderOptions options)
    {
        _logsFolder = options.Folder;
        ArgumentNullException.ThrowIfNull(_logsFolder);
    }

    public async IAsyncEnumerable<LogMessage> LoadItemsFromLogFile(
        [EnumeratorCancellation] CancellationToken cancel = default
    )
    {
        var result = new Stack<LogMessage>();
        await foreach (
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

                var item = Serializer.Deserialize<LogMessage>(rdr);
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

using System.Runtime.CompilerServices;
using System.Text;
using DotNext.Collections.Generic;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Asv.Avalonia;

public class LogReaderService : ILogReaderService, IExportable
{
    private static readonly JsonSerializer Serializer = new();

    private readonly string _logsFolder;

    public LogReaderService(IOptions<LogToFileOptions> options)
    {
        _logsFolder = options.Value.Folder;
        ArgumentNullException.ThrowIfNull(_logsFolder);
    }

    public IExportInfo Source => SystemModule.Instance;

    public async IAsyncEnumerable<LogMessage> LoadItemsFromLogFile(
        [EnumeratorCancellation] CancellationToken cancel = default
    )
    {
        await foreach (var logFilePath in Directory.EnumerateFiles(_logsFolder, "*.logs").Order())
        {
            await using var fs = new FileStream(
                logFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite,
                64 * 1024, // 64 Kb
                FileOptions.Asynchronous | FileOptions.SequentialScan
            );
            using var sr = new StreamReader(fs, Encoding.UTF8, true, 64 * 1024, true);

            var rdr = new JsonTextReader(sr) { SupportMultipleContent = true };

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
                    yield return item;
                }
            }
        }
    }
}

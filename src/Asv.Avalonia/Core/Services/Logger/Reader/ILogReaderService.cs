using System.Diagnostics.CodeAnalysis;

namespace Asv.Avalonia;

public interface ILogReaderService
{
    [RequiresUnreferencedCode(
        "Uses Newtonsoft.Json reflection-based serialization, which is not trim safe."
    )]
    IAsyncEnumerable<LogMessage> LoadItemsFromLogFile(CancellationToken cancel = default);
}

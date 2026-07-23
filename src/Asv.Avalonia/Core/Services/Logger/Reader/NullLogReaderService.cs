using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Asv.Avalonia;

public class NullLogReaderService : ILogReaderService
{
    public static ILogReaderService Instance { get; } = new NullLogReaderService();

    private NullLogReaderService() { }

    /// <inheritdoc />
    [RequiresUnreferencedCode(
        "Uses Newtonsoft.Json reflection-based serialization, which is not trim safe."
    )]
    public async IAsyncEnumerable<LogMessage> LoadItemsFromLogFile(
        [EnumeratorCancellation] CancellationToken cancel = default
    )
    {
        yield break;
    }
}

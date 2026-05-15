using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia;

public class NullLogReaderService : ILogReaderService
{
    public static ILogReaderService Instance { get; } = new NullLogReaderService();

    private NullLogReaderService() { }

    public async IAsyncEnumerable<LogMessage> LoadItemsFromLogFile(
        [EnumeratorCancellation] CancellationToken cancel = default
    )
    {
        yield break;
    }
}

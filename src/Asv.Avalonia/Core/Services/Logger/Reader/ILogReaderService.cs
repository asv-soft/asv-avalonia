namespace Asv.Avalonia;

public interface ILogReaderService
{
    IAsyncEnumerable<LogMessage> LoadItemsFromLogFile(CancellationToken cancel = default);
}

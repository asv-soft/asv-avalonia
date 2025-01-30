using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;
using Observable = System.Reactive.Linq.Observable;

namespace Asv.Avalonia;

public class NullLogService : ILogService
{
    public static NullLogService Instance { get; } = new();

    public ReactiveProperty<LogMessage> OnMessage { get; } = new();

    ReadOnlyReactiveProperty<LogMessage?> ILogService.OnMessage => null!;

    public void SaveMessage(LogMessage logMessage) { }

    public void DeleteLogFile() { }

    IEnumerable<LogMessage> ILogService.LoadItemsFromLogFile()
    {
        return new List<LogMessage>();
    }

    public ILogger CreateLogger(string categoryName)
    {
        return NullLogger.Instance;
    }

    public void AddProvider(ILoggerProvider provider) { }

    public void Dispose() { }
}

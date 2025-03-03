using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia;

public class NullLogService : ILogService
{
    public static ILogService Instance { get; } = new NullLogService();

    private readonly NullLoggerFactory _factory;

    private NullLogService()
    {
        _factory = NullLoggerFactory.Instance;
    }

    public void Dispose()
    {
        _factory.Dispose();
        OnMessage.Dispose();
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _factory.CreateLogger(categoryName);
    }

    public void AddProvider(ILoggerProvider provider)
    {
        _factory.AddProvider(provider);
    }

    public ReadOnlyReactiveProperty<LogMessage?> OnMessage { get; } =
        new ReactiveProperty<LogMessage?>();

    public void SaveMessage(LogMessage logMessage)
    {
        // nothing to do
    }

    public void DeleteLogFile()
    {
        // nothing to do
    }

    public IEnumerable<LogMessage> LoadItemsFromLogFile()
    {
        return ArraySegment<LogMessage>.Empty;
    }
}

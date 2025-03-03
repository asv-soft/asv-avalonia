using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public interface ILogService
{
    ReadOnlyReactiveProperty<LogMessage?> OnMessage { get; }
    void SaveMessage(LogMessage logMessage);
    void DeleteLogFile();
    IEnumerable<LogMessage> LoadItemsFromLogFile();

    public void Fatal(string sender, string message, Exception? ex = null)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogLevel.Critical, sender, message, ex?.Message));
    }

    public void Error(string sender, string message, Exception? ex = null)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogLevel.Error, sender, message, ex?.Message));
    }

    public void Info(string sender, string message)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogLevel.Information, sender, message, null));
    }

    public void Warning(string sender, string message)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogLevel.Warning, sender, message, null));
    }

    public void Trace(string sender, string message)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogLevel.Trace, sender, message, null));
    }

    public void Debug(string sender, string message)
    {
        SaveMessage(new LogMessage(DateTime.Now, LogLevel.Debug, sender, message, null));
    }
}

using Microsoft.Extensions.Logging;
using NuGet.Common;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Asv.Avalonia.Plugins;

public class LoggerAdapter(ILogger logger) : LoggerBase
{
    public override void Log(ILogMessage message)
    {
        switch (message.WarningLevel)
        {
            case WarningLevel.Severe:
                logger.LogError(message.Message);
                break;
            case WarningLevel.Important:
                logger.LogWarning(message.Message);
                break;
            case WarningLevel.Minimal:
                logger.LogInformation(message.Message);
                break;
            case WarningLevel.Default:
                logger.LogDebug(message.Message);
                break;
            default:
                throw new ArgumentOutOfRangeException(message.Message);
        }
    }

    public override Task LogAsync(ILogMessage message)
    {
        Log(message);
        return Task.CompletedTask;
    }
}

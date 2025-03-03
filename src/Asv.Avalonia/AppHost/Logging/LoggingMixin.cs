using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia;

public static class LoggingMixin
{
    public static AppHostBuilder SetLogLevel(this AppHostBuilder builder, LogLevel minLogLevel)
    {
        return builder.SetupLogging(log => log.SetMinimumLevel(minLogLevel));
    }

    public static AppHostBuilder UseLogToConsole(this AppHostBuilder builder)
    {
        return builder.SetupLogging(log =>
        {
            log.AddZLoggerConsole(options =>
            {
                options.IncludeScopes = true;
                options.OutputEncodingToUtf8 = false;
                options.UsePlainTextFormatter(formatter =>
                {
                    formatter.SetPrefixFormatter(
                        $"{0:HH:mm:ss.fff} | ={1:short}= | {2, -40} ",
                        (in MessageTemplate template, in LogInfo info) =>
                            template.Format(info.Timestamp, info.LogLevel, info.Category)
                    );
                });
            });
        });
    }

    public static AppHostBuilder UseLogToConsoleOnDebug(this AppHostBuilder builder)
    {
#if DEBUG
        return builder.UseLogToConsole();
#endif
        return builder;
    }
}

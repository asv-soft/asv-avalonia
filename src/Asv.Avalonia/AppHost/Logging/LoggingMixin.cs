using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia;

public static class LoggingMixin
{
    public static IHostApplicationBuilder SetLogLevel(
        this IHostApplicationBuilder builder,
        LogLevel minLogLevel
    )
    {
        builder.Logging.SetMinimumLevel(minLogLevel);
        return builder;
    }

    public static IHostApplicationBuilder UseLogToConsole(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddZLoggerConsole(options =>
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
        return builder;
    }

    public static IHostApplicationBuilder UseLogToConsoleOnDebug(
        this IHostApplicationBuilder builder
    )
    {
#if DEBUG
        return builder.UseLogToConsole();
#endif
        return builder;
    }
}

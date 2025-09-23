using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZLogger;

namespace Asv.Avalonia;

public static class LoggerMixin
{
    public static IHostApplicationBuilder UseLogging(
        this IHostApplicationBuilder builder,
        Action<LoggerOptionsBuilder>? configure
    )
    {
        var loggerConfig = builder
            .Configuration.GetSection(LoggerOptions.ConfigurationSection)
            .Get<LoggerOptions>();

        var loggerOptionsBuilder = loggerConfig is null
            ? new LoggerOptionsBuilder()
            : new LoggerOptionsBuilder(loggerConfig);

        if (configure is null)
        {
            return builder;
        }

        configure.Invoke(loggerOptionsBuilder);

        var loggerOptions = loggerOptionsBuilder.Build();

        ApplyLogToFile(builder, loggerOptions);
        ApplyLogViewer(builder, loggerOptions);
        ApplyLogToConsole(builder, loggerOptions);
        ApplyLogLevel(builder, loggerOptions);

        builder.Services.AddSingleton(Options.Create(loggerOptions));

        return builder;
    }

    private static void ApplyLogLevel(IHostApplicationBuilder builder, LoggerOptions loggerOptions)
    {
        if (loggerOptions.Level is null)
        {
            return;
        }

        builder.Logging.SetMinimumLevel(loggerOptions.Level.Value);
    }

    private static void ApplyLogViewer(IHostApplicationBuilder builder, LoggerOptions loggerOptions)
    {
        if (loggerOptions.ViewerEnabled is null || !loggerOptions.ViewerEnabled.Value)
        {
            return;
        }

        if (loggerOptions.LogToFileOptions is null)
        {
            throw new ArgumentException(
                "You must configure LogToFile options to enable log viewer"
            );
        }

        builder.Services.AddSingleton<ILogReaderService, LogReaderService>();
    }

    private static void ApplyLogToConsole(
        IHostApplicationBuilder builder,
        LoggerOptions loggerOptions
    )
    {
        if (loggerOptions.LogToConsole is null || !loggerOptions.LogToConsole.Value)
        {
            return;
        }

        builder.Logging.AddZLoggerConsole(options =>
        {
            options.IncludeScopes = true;
            options.OutputEncodingToUtf8 = false;
            options.UsePlainTextFormatter(formatter =>
            {
                formatter.SetPrefixFormatter(
                    $"{0:HH:mm:ss.fff} | {3:00} | ={1:short}= | {2,-40} ",
                    (in MessageTemplate template, in LogInfo info) =>
                        template.Format(
                            info.Timestamp,
                            info.LogLevel,
                            info.Category,
                            Environment.CurrentManagedThreadId
                        )
                );
            });
        });
    }

    private static void ApplyLogToFile(IHostApplicationBuilder builder, LoggerOptions loggerOptions)
    {
        if (loggerOptions.LogToFileOptions is null)
        {
            return;
        }

        if (!Directory.Exists(loggerOptions.LogToFileOptions.Folder))
        {
            Directory.CreateDirectory(loggerOptions.LogToFileOptions.Folder);
        }

        builder.Services.AddSingleton(Options.Create(loggerOptions.LogToFileOptions));

        builder.Logging.AddZLoggerRollingFile(options =>
        {
            options.FilePathSelector = (dt, index) =>
                $"{loggerOptions.LogToFileOptions.Folder}/{dt:yyyy-MM-dd}_{index}.logs";
            options.UseJsonFormatter();
            options.RollingSizeKB = loggerOptions.LogToFileOptions.RollingSizeKb;
        });
    }
}

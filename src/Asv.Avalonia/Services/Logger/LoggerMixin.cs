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
        Action<LoggerOptionsBuilder> configure
    )
    {
        // try to parse config from user setting
        var loggerConfig = builder
            .Configuration.GetSection(LoggerConfig.ConfigurationSection)
            .Get<LoggerConfig>();

        // let user modify this config with builder
        var loggerOptionsBuilder = loggerConfig is null
            ? new LoggerOptionsBuilder()
            : new LoggerOptionsBuilder(loggerConfig);

        configure(loggerOptionsBuilder);

        var loggerOptions = loggerOptionsBuilder.Build();

        if (loggerOptions.LogToFileOptions is not null)
        {
            CreateLogsFolderIfNotExists(loggerOptions.LogToFileOptions.Folder);
            builder.Services.AddSingleton(Options.Create(loggerOptions.LogToFileOptions));
            builder.UseLogToFile(
                loggerOptions.LogToFileOptions.Folder,
                loggerOptions.LogToFileOptions.RollingSizeKb
            );
        }

        if (loggerOptions.ViewerEnabled)
        {
            if (loggerOptions.LogToFileOptions is null)
            {
                throw new ArgumentException(
                    "You must configure LogToFile options to enable log viewer"
                );
            }

            builder.Services.AddSingleton<ILogReaderService, LogReaderService>();
        }

        if (loggerOptions.LogToConsole)
        {
            builder.UseLogToConsole();
        }
        if (loggerOptions.Level is not null)
        {
            builder.Logging.SetMinimumLevel(loggerOptions.Level.Value);
        }

        builder.Services.AddSingleton(Options.Create(loggerOptions));

        return builder;
    }

    private static void UseLogToConsole(this IHostApplicationBuilder builder)
    {
        builder.Logging.AddZLoggerConsole(options =>
        {
            options.IncludeScopes = true;
            options.OutputEncodingToUtf8 = false;
            options.UsePlainTextFormatter(formatter =>
            {
                formatter.SetPrefixFormatter(
                    $"{0:HH:mm:ss.fff} | {3:00} | ={1:short}= | {2, -40} ",
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

    private static void UseLogToFile(
        this IHostApplicationBuilder builder,
        string folderName,
        int rollingSizeKb
    )
    {
        builder.Logging.AddZLoggerRollingFile(options =>
        {
            options.FilePathSelector = (dt, index) => $"{folderName}/{dt:yyyy-MM-dd}_{index}.logs";
            options.UseJsonFormatter();
            options.RollingSizeKB = rollingSizeKb;
        });
    }

    private static void CreateLogsFolderIfNotExists(string logsFolder)
    {
        if (!Directory.Exists(logsFolder))
        {
            Directory.CreateDirectory(logsFolder);
        }
    }
}

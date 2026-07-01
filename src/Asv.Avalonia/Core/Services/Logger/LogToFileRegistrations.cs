using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ZLogger;

namespace Asv.Avalonia;

public class LogToFileOptions
{
    public const string Section = "LogToFile";
    public int RollingSizeKb { get; set; } = 50 * 1024;
    public string Folder { get; set; } = "logs";
}

public static class LogToFileRegistrations
{
    extension(ServicesRegistrations.Builder builder)
    {
        public ServicesRegistrations.Builder RegisterLogToFile(Action<Builder>? configure = null)
        {
            if (builder.AppBuilder.IsDesignTimeEnvironment)
            {
                return builder;
            }

            var logFileOptions =
                builder
                    .AppBuilder.Configuration.GetSection(LogToFileOptions.Section)
                    .Get<LogToFileOptions>()
                ?? new LogToFileOptions();

            configure ??= b => b.UseDefault();
            configure.Invoke(new Builder(logFileOptions));
            logFileOptions.Folder = AppPath.GetAppPathFolder(
                builder.AppBuilder.Environment,
                logFileOptions.Folder
            );
            builder.AppBuilder.Logging.AddZLoggerRollingFile(options =>
            {
                options.FilePathSelector = (dt, index) =>
                    Path.Combine(logFileOptions.Folder, $"{dt:yyyy-MM-dd}_{index}.logs");
                options.UseJsonFormatter();
                options.RollingSizeKB = logFileOptions.RollingSizeKb;
            });

            builder.AppBuilder.Services.AddSingleton(logFileOptions);

            return builder;
        }
    }

    public class Builder(LogToFileOptions options)
    {
        public Builder UseDefault()
        {
            return WithFolder("logs").WithRollingSizeKb(50 * 1024);
        }

        public Builder WithFolder(string folder)
        {
            ArgumentNullException.ThrowIfNull(folder);
            options.Folder = folder;
            return this;
        }

        public Builder WithRollingSizeKb(int rollingSizeKb)
        {
            options.RollingSizeKb = rollingSizeKb;
            return this;
        }
    }
}

using Asv.Cfg;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public sealed class BuilderLoggerOptions : IBuilderOptions
{
    public Func<IConfiguration, LogLevel> LogMinimumLevelCallBack { get; private set; }
    public Func<IConfiguration, string> LogFolderCallback { get; private set; }
    public Func<IConfiguration, int> RollingSizeKbCallback { get; private set; }
    public bool IsLogToConsoleEnabled { get; private set; }

    public BuilderLoggerOptions()
    {
        LogMinimumLevelCallBack = _ => LogLevel.Information;
        LogFolderCallback = _ => string.Empty;
        RollingSizeKbCallback = _ => 0;
    }

    public BuilderLoggerOptions(LogLevel logLevel, string logFolder, int rollingSizeKb)
    {
        LogMinimumLevelCallBack = _ => logLevel;
        LogFolderCallback = _ => logFolder;
        RollingSizeKbCallback = _ => rollingSizeKb;
    }

    /// <summary>
    /// Configures the application host builder with the specified minimum log level.
    /// </summary>
    /// <param name="logLevel">The minimum log level to be used for logging.</param>
    public void WithLogMinimumLevel(LogLevel logLevel)
    {
        LogMinimumLevelCallBack = _ => logLevel;
    }

    /// <summary>
    /// Configures the application host builder with the specified minimum logging level.
    /// </summary>
    /// <param name="fromConfig">A function that extracts the minimum log level from a configuration object.</param>
    /// <typeparam name="TConfig">The type of the configuration object. It will be loaded from the <see cref="IConfiguration"/>.</typeparam>
    public void WithLogMinimumLevel<TConfig>(Func<TConfig, LogLevel> fromConfig)
        where TConfig : new()
    {
        LogMinimumLevelCallBack = x => fromConfig(x.Get<TConfig>());
    }

    /// <summary>
    /// Configures the application host builder to use a JSON log file stored in the specified folder with a rolling file size limit.
    /// </summary>
    /// <typeparam name="TConfig">The type used for retrieving configuration values.</typeparam>
    /// <param name="logFolder">A function that provides the folder path for log files based on the configuration.</param>
    /// <param name="rollingSizeKb">A function that specifies the file size limit, in kilobytes, for rolling log files based on the configuration.</param>
    public void WithJsonLogFolder<TConfig>(
        Func<TConfig, string> logFolder,
        Func<TConfig, int> rollingSizeKb
    )
        where TConfig : new()
    {
        LogFolderCallback = x => logFolder(x.Get<TConfig>());
        RollingSizeKbCallback = x => rollingSizeKb(x.Get<TConfig>());
    }

    /// <summary>
    /// Sets the folder for JSON log storage and configures the rolling log file size in kilobytes.
    /// </summary>
    /// <param name="logFolder">The path to the folder where log files will be stored.</param>
    /// <param name="rollingSizeKb">A function that retrieves the rolling file size in kilobytes from the configuration.</param>
    /// <typeparam name="TConfig">The type of the configuration class.</typeparam>
    public void WithJsonLogFolder<TConfig>(string logFolder, Func<TConfig, int> rollingSizeKb)
        where TConfig : new()
    {
        LogFolderCallback = _ => logFolder;
        RollingSizeKbCallback = x => rollingSizeKb(x.Get<TConfig>());
    }

    /// <summary>
    /// Configures the application host builder to use a JSON log folder with the specified directory path and file size rolling threshold.
    /// </summary>
    /// <param name="logFolder">The directory path where the JSON log files will be stored.</param>
    /// <param name="rollingSizeKb">The maximum size of a log file in kilobytes before it rolls over to a new file.</param>
    public void WithJsonLogFolder(string logFolder, int rollingSizeKb)
    {
        LogFolderCallback = _ => logFolder;
        RollingSizeKbCallback = _ => rollingSizeKb;
    }

    /// <summary>
    /// Enables or disables logging to the console for the application.
    /// </summary>
    /// <param name="enabled">A boolean value indicating whether logging to the console is enabled. Defaults to true.</param>
    public void WithLogToConsole(bool enabled = true)
    {
        IsLogToConsoleEnabled = enabled;
    }
}

public static class AppBuilderLoggerExtensions
{
    public static IAppHostBuilder UseLogging(
        this IAppHostBuilder builder,
        BuilderLoggerOptions options
    )
    {
        builder.Options.Add(typeof(BuilderLoggerOptions), options);
        return builder;
    }

    public static IAppHostBuilder UseLogging(
        this IAppHostBuilder builder,
        Action<BuilderLoggerOptions>? setupAction = null
    )
    {
        var options = new BuilderLoggerOptions();

        setupAction?.Invoke(options);

        return builder.UseLogging(options);
    }
}

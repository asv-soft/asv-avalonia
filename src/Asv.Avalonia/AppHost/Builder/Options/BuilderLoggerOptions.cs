using Asv.Cfg;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public sealed class BuilderLoggerOptions : IBuilderOptions
{
    public Func<IConfiguration, LogLevel> LogMinimumLevelCallBack { get; set; } =
        _ => LogLevel.Information;
    public Func<IConfiguration, string?> LogFolderCallback { get; set; } = _ => null;
    public Func<IConfiguration, int> RollingSizeKbCallback { get; set; } = _ => 0;
    public bool IsLogToConsoleEnabled { get; set; }
}

public static class BuilderLoggerOptionsExtensions
{
    /// <summary>
    /// Configures the application host builder with the specified minimum log level.
    /// </summary>
    /// <param name="logLevel">The minimum log level to be used for logging.</param>
    public static void WithLogMinimumLevel(this BuilderLoggerOptions options, LogLevel logLevel)
    {
        options.LogMinimumLevelCallBack = _ => logLevel;
    }

    /// <summary>
    /// Configures the application host builder with the specified minimum logging level.
    /// </summary>
    /// <param name="fromConfig">A function that extracts the minimum log level from a configuration object.</param>
    /// <typeparam name="TConfig">The type of the configuration object. It will be loaded from the <see cref="IConfiguration"/>.</typeparam>
    public static void WithLogMinimumLevel<TConfig>(
        this BuilderLoggerOptions options,
        Func<TConfig, LogLevel> fromConfig
    )
        where TConfig : new()
    {
        options.LogMinimumLevelCallBack = x => fromConfig(x.Get<TConfig>());
    }

    /// <summary>
    /// Configures the application host builder to use a JSON log file stored in the specified folder with a rolling file size limit.
    /// </summary>
    /// <typeparam name="TConfig">The type used for retrieving configuration values.</typeparam>
    /// <param name="logFolder">A function that provides the folder path for log files based on the configuration.</param>
    /// <param name="rollingSizeKb">A function that specifies the file size limit, in kilobytes, for rolling log files based on the configuration.</param>
    public static void WithJsonLogFolder<TConfig>(
        this BuilderLoggerOptions options,
        Func<TConfig, string> logFolder,
        Func<TConfig, int> rollingSizeKb
    )
        where TConfig : new()
    {
        options.LogFolderCallback = x => logFolder(x.Get<TConfig>());
        options.RollingSizeKbCallback = x => rollingSizeKb(x.Get<TConfig>());
    }

    /// <summary>
    /// Sets the folder for JSON log storage and configures the rolling log file size in kilobytes.
    /// </summary>
    /// <param name="logFolder">The path to the folder where log files will be stored.</param>
    /// <param name="rollingSizeKb">A function that retrieves the rolling file size in kilobytes from the configuration.</param>
    /// <typeparam name="TConfig">The type of the configuration class.</typeparam>
    public static void WithJsonLogFolder<TConfig>(
        this BuilderLoggerOptions options,
        string logFolder,
        Func<TConfig, int> rollingSizeKb
    )
        where TConfig : new()
    {
        options.LogFolderCallback = _ => logFolder;
        options.RollingSizeKbCallback = x => rollingSizeKb(x.Get<TConfig>());
    }

    /// <summary>
    /// Configures the application host builder to use a JSON log folder with the specified directory path and file size rolling threshold.
    /// </summary>
    /// <param name="logFolder">The directory path where the JSON log files will be stored.</param>
    /// <param name="rollingSizeKb">The maximum size of a log file in kilobytes before it rolls over to a new file.</param>
    public static void WithJsonLogFolder(
        this BuilderLoggerOptions options,
        string logFolder,
        int rollingSizeKb
    )
    {
        options.LogFolderCallback = _ => logFolder;
        options.RollingSizeKbCallback = _ => rollingSizeKb;
    }

    /// <summary>
    /// Enables or disables logging to the console for the application.
    /// </summary>
    /// <param name="enabled">A boolean value indicating whether logging to the console is enabled. Defaults to true.</param>
    public static void WithLogToConsole(this BuilderLoggerOptions options, bool enabled = true)
    {
        options.IsLogToConsoleEnabled = enabled;
    }
}

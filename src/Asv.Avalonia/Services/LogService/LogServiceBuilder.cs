using System.Reflection;
using Asv.Cfg;

namespace Asv.Avalonia;

public static class LogServiceBuilder
{
    public static LogService BuildFromOptions(IConfiguration cfg, BuilderLoggerOptions options)
    {
        ArgumentNullException.ThrowIfNull(cfg);
        var logLevel = options.LogMinimumLevelCallBack(cfg);
        var logFolder =
            options.LogFolderCallback(cfg)
            ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
            ?? string.Empty;
        var rollingSizeKb = options.RollingSizeKbCallback(cfg);
        var isLogToConsoleEnabled = options.IsLogToConsoleEnabled;
        return new LogService(logFolder, rollingSizeKb, logLevel, isLogToConsoleEnabled);
    }
}

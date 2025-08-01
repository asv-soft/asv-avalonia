using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class LoggerConfig
{
    public const string ConfigurationSection = "Logs";

    public required bool? ViewerEnabled { get; init; } = false;
    public required bool? LogToConsole { get; init; } = false;
    public required LogToFileOptions? LogToFileOptions { get; init; }
    public required LogLevel? Level { get; init; }
}

public class LogToFileOptions
{
    public LogToFileOptions(string? folder, int? rollingSizeKb)
    {
        if (folder is not null)
        {
            Folder = folder;
        }
        if (rollingSizeKb is not null)
        {
            RollingSizeKb = rollingSizeKb.Value;
        }
    }

    public string Folder { get; } = "logs";
    public int RollingSizeKb { get; } = 50;
}

public class LoggerOptions
{
    public required bool ViewerEnabled { get; init; }
    public required bool LogToConsole { get; init; }
    public required LogToFileOptions? LogToFileOptions { get; init; }
    public required LogLevel? Level { get; init; }
}

public class LoggerOptionsBuilder
{
    public LoggerOptionsBuilder() { }

    public LoggerOptionsBuilder(LoggerConfig defaultOptions)
    {
        ViewerEnabled = defaultOptions.ViewerEnabled ?? ViewerEnabled;
        LogToConsole = defaultOptions.LogToConsole ?? LogToConsole;
        LogToFileOptions = defaultOptions.LogToFileOptions;
        Level = defaultOptions.Level;
    }

    public bool ViewerEnabled { get; set; }
    public bool LogToConsole { get; set; }
    public LogToFileOptions? LogToFileOptions { get; set; }
    public LogLevel? Level { get; set; }

    public LoggerOptionsBuilder WithLogToConsole()
    {
        LogToConsole = true;
        return this;
    }

    public LoggerOptionsBuilder WithLogToFile(string? folderName = null, int? rollingSizeKb = null)
    {
        LogToFileOptions = new LogToFileOptions(folderName, rollingSizeKb);
        return this;
    }

    public LoggerOptionsBuilder WithLogViewer()
    {
        ViewerEnabled = true;
        return this;
    }

    public LoggerOptionsBuilder WithLogLevel(LogLevel minLogLevel)
    {
        Level = minLogLevel;
        return this;
    }

    public LoggerOptions Build()
    {
        return new LoggerOptions
        {
            ViewerEnabled = ViewerEnabled,
            LogToConsole = LogToConsole,
            LogToFileOptions = LogToFileOptions,
            Level = Level,
        };
    }
}

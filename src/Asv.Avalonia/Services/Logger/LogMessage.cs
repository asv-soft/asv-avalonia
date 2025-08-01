using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class LogMessage(
    DateTime timestamp,
    LogLevel logLevel,
    string category,
    string message,
    string? description
)
{
    public DateTime Timestamp { get; } = timestamp;
    public LogLevel LogLevel { get; } = logLevel;
    public string Category { get; internal set; } = category;
    public string Message { get; } = message;
    public string? Description { get; } = description;

    public override string ToString()
    {
        return $"{Category} {Message}";
    }
}

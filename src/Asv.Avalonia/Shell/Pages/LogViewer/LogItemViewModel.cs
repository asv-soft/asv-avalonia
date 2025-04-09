﻿using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class LogItemViewModel(long itemIndex, LogMessage msg)
    : RoutableViewModel($"log_item{itemIndex}")
{
    public long Index { get; } = itemIndex;

    public MaterialIconKind Kind { get; } =
        msg.LogLevel switch
        {
            LogLevel.Debug => MaterialIconKind.Bug,
            LogLevel.Trace => MaterialIconKind.Tractor,
            LogLevel.Information => MaterialIconKind.Info,
            LogLevel.Warning => MaterialIconKind.Bullhorn,
            LogLevel.Error => MaterialIconKind.Fire,
            LogLevel.Critical => MaterialIconKind.FlashAlert,
            _ => MaterialIconKind.QuestionMark,
        };

    public DateTime Timestamp => msg.Timestamp;
    public LogLevel Level => msg.LogLevel;
    public string Category => msg.Category;
    public string Message => msg.Message;

    public bool IsTrace { get; } = msg.LogLevel == LogLevel.Trace;
    public bool IsDebug { get; } = msg.LogLevel == LogLevel.Debug;
    public bool IsInfo { get; } = msg.LogLevel == LogLevel.Information;
    public bool IsWarning { get; } = msg.LogLevel == LogLevel.Warning;
    public bool IsError { get; } = msg.LogLevel == LogLevel.Error;
    public bool IsFatal { get; } = msg.LogLevel == LogLevel.Critical;
    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }
}

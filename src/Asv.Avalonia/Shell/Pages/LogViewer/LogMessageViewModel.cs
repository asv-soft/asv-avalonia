using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class LogMessageViewModel : RoutableViewModel
{
    private readonly ILoggerFactory _loggerFactory;

    public LogMessageViewModel(
        LogMessage @base,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IRoutable parent
    )
        : base(
            NavigationId.GenerateByHash(@base.Message, @base.Category, @base.Description),
            layoutService,
            loggerFactory
        )
    {
        _loggerFactory = loggerFactory;
        Base = @base;
        Parent = parent;
    }

    public LogMessage Base { get; }

    public Selection MessageSelection { get; init; }
    public Selection CategorySelection { get; init; }

    public MaterialIconKind Icon =>
        Base.LogLevel switch
        {
            LogLevel.Critical => MaterialIconKind.AlertCircle,
            LogLevel.Error => MaterialIconKind.Alert,
            LogLevel.Information => MaterialIconKind.Information,
            LogLevel.Warning => MaterialIconKind.AlertOutline,
            LogLevel.Trace => MaterialIconKind.Bug,
            LogLevel.Debug => MaterialIconKind.CodeBraces,
            _ => MaterialIconKind.HelpCircle,
        };

    public bool IsTrace => Base.LogLevel is LogLevel.Trace or LogLevel.None;
    public bool IsDebug => Base.LogLevel == LogLevel.Debug;
    public bool IsInformation => Base.LogLevel == LogLevel.Information;
    public bool IsWarning => Base.LogLevel == LogLevel.Warning;
    public bool IsError => Base.LogLevel is LogLevel.Error or LogLevel.Critical;
    public string DateTime => Base.Timestamp.ToString("yy-MM-dd HH:mm:ss.fff");

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void Dispose(bool disposing)
    {
        // No resources to dispose
    }

    public static bool TryCreate(
        LogMessage logMessage,
        ISearchService search,
        string? query,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IRoutable parent,
        out LogMessageViewModel? msg
    )
    {
        msg = null;
        var catSelection = Selection.Empty;

        var result =
            search.Match(logMessage.Message, query, out var msgSelection)
            || search.Match(logMessage.Category, query, out catSelection);
        if (!result)
        {
            return false;
        }

        msg = new LogMessageViewModel(logMessage, layoutService, loggerFactory, parent)
        {
            MessageSelection = msgSelection,
            CategorySelection = catSelection,
        };

        return true;
    }
}

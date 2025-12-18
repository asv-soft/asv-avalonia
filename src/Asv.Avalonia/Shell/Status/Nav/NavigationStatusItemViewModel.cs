using System.Composition;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

[ExportStatusItem]
public class NavigationStatusItemViewModel : StatusItem
{
    private readonly ObservableList<string> _source;
    private readonly ICommandService _commandService;
    public const string StaticId = "nav-crumbs";

    public NavigationStatusItemViewModel()
        : base(StaticId, DesignTime.LoggerFactory)
    {
        _commandService = DesignTime.CommandService;
        _source = new ObservableList<string>();
        Items = _source.ToNotifyCollectionChangedSlim();
        CommandInfo = NextPageCommand.StaticInfo;
        CommandHotKey = NextPageCommand.StaticInfo.DefaultHotKey?.ToString() ?? string.Empty;
        _source.Add("shell");
        _source.Add("tab1");
        _source.Add("element1");
    }

    [ImportingConstructor]
    public NavigationStatusItemViewModel(
        ILoggerFactory loggerFactory,
        INavigationService nav,
        ICommandService commandService
    )
        : base(StaticId, loggerFactory)
    {
        _source = new ObservableList<string>();
        Items = _source.ToNotifyCollectionChangedSlim();
        nav.SelectedControl.Subscribe(OnChanged).AddTo(Disposable);
        _commandService = commandService;
        commandService.OnCommand.Subscribe(OnCommand).AddTo(Disposable);
    }

    private void OnChanged(IRoutable? routable)
    {
        _source.Clear();
        if (routable == null)
        {
            return;
        }

        foreach (var item in routable.GetHierarchyFromRoot().OfType<IRoutable>())
        {
            _source.Add(item.Id.Id);
        }
    }

    private void OnCommand(CommandSnapshot commandSnapshot)
    {
        CommandInfo = _commandService.GetCommandInfo(commandSnapshot.CommandId);
        CommandHotKey =
            _commandService.GetHotKey(commandSnapshot.CommandId)?.ToString() ?? string.Empty;
    }

    public ICommandInfo? CommandInfo
    {
        get;
        private set => SetField(ref field, value);
    }

    public string? CommandHotKey
    {
        get;
        set => SetField(ref field, value);
    }

    public NotifyCollectionChangedSynchronizedViewList<string> Items { get; }

    public override IEnumerable<IRoutable> GetChildren()
    {
        return [];
    }

    public override int Order { get; } = int.MaxValue;
}

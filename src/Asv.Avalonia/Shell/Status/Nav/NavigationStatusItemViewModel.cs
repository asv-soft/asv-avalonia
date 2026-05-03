using Asv.Common;
using Asv.Modeling;
using Avalonia.Controls;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class NavigationStatusItemViewModel : StatusItem
{
    private readonly ObservableList<string> _source;
    public const string StaticId = "nav_crumbs";

    public NavigationStatusItemViewModel()
        : base(StaticId, default)
    {
        _source = new ObservableList<string>();
        Items = _source.ToNotifyCollectionChangedSlim();
        CommandInfo = NextPageCommand.StaticInfo;
        CommandHotKey = NextPageCommand.StaticInfo.DefaultHotKey?.ToString() ?? string.Empty;
        _source.Add("shell");
        _source.Add("tab1");
        _source.Add("element1");
    }

    public NavigationStatusItemViewModel(IShellHost nav)
        : base(StaticId, default)
    {
        _source = new ObservableList<string>();
        Items = _source.ToNotifyCollectionChangedSlim();
        nav.ExecuteNowOrWhenShellLoaded(OnShellLoaded).AddTo(Disposable);
        
        _commandService = commandService;
        commandService.OnCommand.Subscribe(OnCommand).AddTo(Disposable);
    }

    private void OnShellLoaded(IShell shell, TopLevel top)
    {
        shell.Navigation.SelectedControl.Subscribe(OnChanged).AddTo(Disposable);
    }

    private void OnChanged(IViewModel? routable)
    {
        _source.Clear();
        if (routable == null)
        {
            return;
        }

        foreach (var item in routable.GetHierarchyFromRoot().OfType<IViewModel>())
        {
            _source.Add(item.Id.TypeId);
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

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }

    public override int Order { get; } = int.MaxValue;
}

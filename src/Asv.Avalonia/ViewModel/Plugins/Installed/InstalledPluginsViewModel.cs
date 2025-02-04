using System.Composition;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

[ExportPage(PageId)]
public class InstalledPluginsViewModel : PageViewModel<InstalledPluginsViewModel>
{
    public const string PageId = "installed_plugins";
    private readonly ILogService _log;
    private readonly IPluginManager _manager;
    protected readonly ObservableList<ILocalPluginInfo> Plugins;

    public InstalledPluginsViewModel()
        : this(DesignTime.CommandService, DesignTime.PluginManager, DesignTime.Log)
    {
        DesignTime.ThrowIfNotDesignMode();
        Plugins = new ObservableList<ILocalPluginInfo>();
        PluginsView = Plugins
            .CreateView<InstalledPluginInfoViewModel>(_ => new InstalledPluginInfoViewModel())
            .ToNotifyCollectionChanged();
        SelectedPlugin = new BindableReactiveProperty<InstalledPluginInfoViewModel>(PluginsView[0]);
    }

    [ImportingConstructor]
    public InstalledPluginsViewModel(ICommandService cmd, IPluginManager manager, ILogService log)
        : base(PageId, cmd)
    {
        _log = log;
        _manager = manager;
        Plugins = new ObservableList<ILocalPluginInfo>();

        Search = new ReactiveCommand(_ => SearchImpl());

        SearchString = new BindableReactiveProperty<string>();
        SelectedPlugin = new BindableReactiveProperty<InstalledPluginInfoViewModel>();
        OnlyVerified = new BindableReactiveProperty<bool>(false);

        PluginsView = Plugins
            .CreateView(info => new InstalledPluginInfoViewModel(
                $"{PageId}[{info.Id}]",
                manager,
                info,
                log
            ))
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
    }

    public ReactiveCommand Search { get; set; }
    public NotifyCollectionChangedSynchronizedViewList<InstalledPluginInfoViewModel> PluginsView { get; set; }
    public BindableReactiveProperty<string> SearchString { get; set; }
    public BindableReactiveProperty<InstalledPluginInfoViewModel> SelectedPlugin { get; set; }
    public BindableReactiveProperty<bool> OnlyVerified { get; set; }

    private void SearchImpl()
    {
        Plugins.Clear();
        Plugins.AddRange(
            OnlyVerified.Value
                ? _manager.Installed.Where(item => item.IsVerified)
                : _manager.Installed
        );
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        throw new NotImplementedException();
    }

    protected override void AfterLoadExtensions()
    {
        throw new NotImplementedException();
    }
}

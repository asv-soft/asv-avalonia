using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class InstalledPluginsViewModel : DisposableViewModel
{
    private readonly ILogService _log;
    private readonly IPluginManager _manager;
    protected readonly ObservableList<ILocalPluginInfo> Plugins;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public InstalledPluginsViewModel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        : base(string.Empty)
    {
        DesignTime.ThrowIfNotDesignMode();
        Plugins = new ObservableList<ILocalPluginInfo>();
        PluginsView = Plugins
            .CreateView<InstalledPluginInfoViewModel>(_ => new InstalledPluginInfoViewModel())
            .ToNotifyCollectionChanged();
        SelectedPlugin = new BindableReactiveProperty<InstalledPluginInfoViewModel>(PluginsView[0]);
    }

    protected InstalledPluginsViewModel(string id, IPluginManager manager, ILogService log)
        : base(id)
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
                $"id{id}.{info.Id}",
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
}

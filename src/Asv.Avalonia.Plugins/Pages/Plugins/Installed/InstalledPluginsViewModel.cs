using System.Composition;
using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Plugins;

[ExportPage(PageId)]
public class InstalledPluginsViewModel : PageViewModel<InstalledPluginsViewModel>
{
    public const string PageId = "plugins.installed";
    public const MaterialIconKind PageIcon = MaterialIconKind.Plugin;

    private readonly IPluginManager _manager;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IConfiguration _cfg;
    private readonly INavigationService _navigation;
    protected readonly ObservableList<ILocalPluginInfo> Plugins;

    public InstalledPluginsViewModel()
        : this(
            DesignTime.CommandService,
            NullPluginManager.Instance,
            NullLoggerFactory.Instance,
            new InMemoryConfiguration(),
            NullNavigationService.Instance
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        Plugins = new ObservableList<ILocalPluginInfo>();
        PluginsView = Plugins
            .CreateView<InstalledPluginInfoViewModel>(_ => new InstalledPluginInfoViewModel())
            .ToNotifyCollectionChanged();
        SelectedPlugin = new BindableReactiveProperty<InstalledPluginInfoViewModel>(PluginsView[0]);
    }

    [ImportingConstructor]
    public InstalledPluginsViewModel(
        ICommandService cmd,
        IPluginManager manager,
        ILoggerFactory loggerFactory,
        IConfiguration cfg,
        INavigationService navigationService
    )
        : base(PageId, cmd)
    {
        _manager = manager;
        _loggerFactory = loggerFactory;
        _navigation = navigationService;
        _cfg = cfg;
        Plugins = new ObservableList<ILocalPluginInfo>();

        Search = new ReactiveCommand(_ => SearchImpl());

        SearchString = new BindableReactiveProperty<string>();
        SelectedPlugin = new BindableReactiveProperty<InstalledPluginInfoViewModel>();
        OnlyVerified = new BindableReactiveProperty<bool>(false);

        InstallManually = new ReactiveCommand<IProgress<double>>(_ =>
            Task.FromResult(InstallManuallyImpl())
        );
        PluginsView = Plugins
            .CreateView(info => new InstalledPluginInfoViewModel(
                $"{PageId}[{info.Id}]",
                manager,
                info,
                loggerFactory
            ))
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
    }

    public ReactiveCommand Search { get; set; }
    public ReactiveCommand<IProgress<double>> InstallManually { get; }
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

    private async Task InstallManuallyImpl()
    {
        var installer = new PluginInstaller(_cfg, _loggerFactory, _manager, _navigation);
        await installer.ShowInstallDialog();
    }

    public override ValueTask<IRoutable> Navigate(NavigationId id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => SystemModule.Instance;
}

using Asv.Cfg;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class PluginsMarketViewModel : DisposableViewModel
{
    private readonly IPluginManager _manager;
    private readonly ILogService _log;
    private readonly ObservableList<PluginInfoViewModel> _plugins;
    private readonly IConfiguration _cfg;
    private string _previouslySelectedPluginId;

    public PluginsMarketViewModel()
        : base(string.Empty)
    {
        DesignTime.ThrowIfNotDesignMode();
        _plugins = new ObservableList<PluginInfoViewModel>(
            new[]
            {
                new PluginInfoViewModel
                {
                    Id = "#1",
                    Author = "Asv Soft",
                    SourceName = "Nuget",
                    Name = "Example1",
                    Description = "Example plugin",
                    LastVersion = "1.0.0",
                    IsInstalled = new BindableReactiveProperty<bool>(true),
                    LocalVersion = "3.4.5",
                    IsVerified = new BindableReactiveProperty<bool>(true),
                },
                new PluginInfoViewModel
                {
                    Id = "#2",
                    Author = "Asv Soft",
                    SourceName = "Github",
                    Name = "Example2",
                    Description = "Example plugin",
                    LastVersion = "0.1.0",
                },
            }
        );
        PluginsView = _plugins.CreateView(x => x).ToNotifyCollectionChanged();
        SelectedPlugin = new BindableReactiveProperty<PluginInfoViewModel?>(_plugins[0]);
    }

    public PluginsMarketViewModel(
        string id,
        IPluginManager manager,
        ILogService log,
        IConfiguration cfg
    )
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(log);
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _log = log;
        _cfg = cfg;

        _plugins = new ObservableList<PluginInfoViewModel>();

        OnlyVerified = new BindableReactiveProperty<bool>(true);
        SearchString = new BindableReactiveProperty<string>();
        SelectedPlugin = new BindableReactiveProperty<PluginInfoViewModel?>();

        PluginsView = _plugins.CreateView(x => x).ToNotifyCollectionChanged();
        Search = new CancellableCommandWithProgress(SearchImpl, "Search", log);
        InstallManually = new ReactiveCommand<IProgress<double>>(_ =>
            Task.FromResult(InstallManuallyImpl())
        );
    }

    public CancellableCommandWithProgress Search { get; }
    public ReactiveCommand<IProgress<double>> InstallManually { get; }
    public NotifyCollectionChangedSynchronizedViewList<PluginInfoViewModel> PluginsView { get; set; }
    public BindableReactiveProperty<bool> OnlyVerified { get; set; }
    public BindableReactiveProperty<string> SearchString { get; set; }
    public BindableReactiveProperty<PluginInfoViewModel?> SelectedPlugin { get; set; }

    private async Task SearchImpl(IProgress<double> progress, CancellationToken cancel)
    {
        var items = await _manager.Search(SearchQuery.Empty, cancel);

        if (SelectedPlugin.Value is not null)
        {
            _previouslySelectedPluginId = SelectedPlugin.Value.Id;
        }

        SelectedPlugin.OnNext(null);
        _plugins.Clear();
        var filteredItems = OnlyVerified.Value ? items.Where(item => item.IsVerified) : items;
        _plugins.AddRange(
            filteredItems.Select(item => new PluginInfoViewModel(
                $"id{item.Id}",
                item,
                _manager,
                _log
            ))
        );
        SelectedPlugin.OnNext(
            _plugins.FirstOrDefault(plugin => plugin.Id == _previouslySelectedPluginId)
                ?? _plugins[0]
        );
    }

    private async Task InstallManuallyImpl()
    {
        var installer = new PluginInstaller(_cfg, _log, _manager);
        await installer.ShowInstallDialog($"{Id}.install_dialog");
    }
}

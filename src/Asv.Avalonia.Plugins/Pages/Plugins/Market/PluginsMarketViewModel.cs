using System.Composition;
using Asv.Cfg;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Plugins;

public sealed class PluginsMarketViewModelConfig : PageConfig { }

[ExportPage(PageId)]
public class PluginsMarketViewModel
    : PageViewModel<PluginsMarketViewModel, PluginsMarketViewModelConfig>
{
    public const string PageId = "plugins.market";

    private readonly IPluginManager _manager;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ObservableList<PluginInfoViewModel> _plugins;
    private readonly IConfiguration _cfg;
    private string _previouslySelectedPluginId;

    public PluginsMarketViewModel()
        : this(
            DesignTime.CommandService,
            NullPluginManager.Instance,
            NullLoggerFactory.Instance,
            new JsonConfiguration("null"),
            DesignTime.DialogService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        _plugins = new ObservableList<PluginInfoViewModel>(
            [
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
            ]
        );
        PluginsView = _plugins.CreateView(x => x).ToNotifyCollectionChanged();
        SelectedPlugin = new BindableReactiveProperty<PluginInfoViewModel?>(_plugins[0]);
    }

    [ImportingConstructor]
    public PluginsMarketViewModel(
        ICommandService cmd,
        IPluginManager manager,
        ILoggerFactory loggerFactory,
        IConfiguration cfg,
        IDialogService dialogService
    )
        : base(PageId, cmd, cfg, loggerFactory, dialogService)
    {
        Title = "Plugin Manager";
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        _cfg = cfg;

        _plugins = new ObservableList<PluginInfoViewModel>();
        _loggerFactory = loggerFactory;

        OnlyVerified = new BindableReactiveProperty<bool>(true);
        SearchString = new BindableReactiveProperty<string>();
        SelectedPlugin = new BindableReactiveProperty<PluginInfoViewModel?>();

        PluginsView = _plugins.CreateView(x => x).ToNotifyCollectionChanged();
        Search = new CancellableCommandWithProgress<Unit>(SearchImpl, "Search", loggerFactory);
    }

    public CancellableCommandWithProgress<Unit> Search { get; }
    public NotifyCollectionChangedSynchronizedViewList<PluginInfoViewModel> PluginsView { get; set; }
    public BindableReactiveProperty<bool> OnlyVerified { get; set; }
    public BindableReactiveProperty<string> SearchString { get; set; }
    public BindableReactiveProperty<PluginInfoViewModel?> SelectedPlugin { get; set; }

    private async Task SearchImpl(Unit arg, IProgress<double> progress, CancellationToken cancel)
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
                _loggerFactory
            ))
        );
        SelectedPlugin.OnNext(
            _plugins.FirstOrDefault(plugin => plugin.Id == _previouslySelectedPluginId)
                ?? _plugins[0]
        );
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

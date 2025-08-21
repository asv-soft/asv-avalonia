using System.Composition;
using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Packaging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Plugins;

[ExportPage(PageId)]
public class PluginsMarketViewModel : PageViewModel<PluginsMarketViewModel>
{
    public const string PageId = "plugins.market";
    public const MaterialIconKind PageIcon = MaterialIconKind.Store;

    private readonly IPluginManager _manager;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ObservableList<PluginInfoViewModel> _plugins;
    private readonly IConfiguration _cfg;

    public PluginsMarketViewModel()
        : this(
            DesignTime.CommandService,
            NullPluginManager.Instance,
            NullLoggerFactory.Instance,
            DesignTime.Configuration,
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
        : base(PageId, cmd, loggerFactory, dialogService)
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
        var query = new SearchQuery
        {
            Name = string.IsNullOrWhiteSpace(SearchString.Value) ? null : SearchString.Value,
            IncludePrerelease = true, // TODO: add BindableReactiveProperty<bool> IncludePrerelease
            Skip = 0,
            Take = 50,
        };
        foreach (var server in _manager.Servers)
        {
            query.Sources.Add(server.SourceUri);
        }
        progress.Report(0);
        var items = await _manager.Search(query, cancel);

        var selectedId = SelectedPlugin.Value?.Id;
        SelectedPlugin.OnNext(null);
        _plugins.Clear();
        var filtered = OnlyVerified.Value ? items.Where(x => x.IsVerified) : items;
        _plugins.AddRange(
            filtered.Select(info => new PluginInfoViewModel(
                $"id{info.Id}",
                info,
                _manager,
                _loggerFactory
            ))
        );
        var first = _plugins.FirstOrDefault(x => x.Id == selectedId) ?? _plugins.FirstOrDefault();
        SelectedPlugin.OnNext(first);
        progress.Report(1);
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

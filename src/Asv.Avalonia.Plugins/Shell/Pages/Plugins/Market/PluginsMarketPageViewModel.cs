using System.Composition;
using Asv.Cfg;
using Asv.Common;
using Avalonia.Threading;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Plugins;

public sealed class PluginsMarketViewModelConfig : PageConfig { }

[ExportPage(PageId)]
public class PluginsMarketPageViewModel
    : PageViewModel<PluginsMarketPageViewModel, PluginsMarketViewModelConfig>
{
    public const string PageId = "plugins.market";
    public const MaterialIconKind PageIcon = MaterialIconKind.Store;

    private readonly IPluginManager _manager;
    private readonly ObservableList<IPluginSearchInfo> _plugins;
    private readonly ISynchronizedView<IPluginSearchInfo, PluginInfoViewModel> _view;

    public PluginsMarketPageViewModel()
        : this(
            DesignTime.CommandService,
            NullPluginManager.Instance,
            NullLoggerFactory.Instance,
            DesignTime.Configuration
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        _plugins.Add(NullPluginSearchInfo.Instance);
    }

    [ImportingConstructor]
    public PluginsMarketPageViewModel(
        ICommandService cmd,
        IPluginManager manager,
        ILoggerFactory loggerFactory,
        IConfiguration cfg
    )
        : base(PageId, cmd, cfg, loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(cmd);
        ArgumentNullException.ThrowIfNull(manager);

        Title = RS.PluginsMarketViewModel_Title;
        Icon = PageIcon;

        _manager = manager;

        _plugins = [];

        var isShowOnlyVerified = new ReactiveProperty<bool>(true).DisposeItWith(Disposable);
        IsShowOnlyVerified = new HistoricalBoolProperty(
            nameof(IsShowOnlyVerified),
            isShowOnlyVerified,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        SelectedPlugin = new BindableReactiveProperty<PluginInfoViewModel?>().DisposeItWith(
            Disposable
        );

        _view = _plugins
            .CreateView(info => new PluginInfoViewModel(info, _manager, loggerFactory))
            .DisposeItWith(Disposable);
        _view.SetRoutableParent(this).DisposeItWith(Disposable);
        _view.DisposeMany().DisposeItWith(Disposable);
        PluginsView = _view
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);

        Search = new SearchBoxViewModel(
            nameof(Search),
            loggerFactory,
            SearchImpl,
            TimeSpan.FromMilliseconds(500)
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        IsShowOnlyVerified
            .ViewValue.ObserveOnUIThreadDispatcher()
            .Skip(1)
            .Subscribe(_ => Search.Refresh())
            .DisposeItWith(Disposable);
    }

    public SearchBoxViewModel Search { get; }
    public NotifyCollectionChangedSynchronizedViewList<PluginInfoViewModel> PluginsView { get; }
    public HistoricalBoolProperty IsShowOnlyVerified { get; }
    public BindableReactiveProperty<PluginInfoViewModel?> SelectedPlugin { get; }

    private async Task SearchImpl(
        string? text,
        IProgress<double> progress,
        CancellationToken cancel
    )
    {
        var query = new SearchQuery
        {
            Name = text,
            IncludePrerelease = true, // TODO: add Historical IncludePrerelease
            Skip = 0, // TODO: add Historical Skip
            Take = 50, // TODO: add Historical Take
        };

        foreach (var server in _manager.Servers)
        {
            query.Sources.Add(server.SourceUri);
        }

        var items = await _manager.Search(
            query,
            new Progress<ProgressMessage>(m => progress.Report(m.Progress)),
            cancel
        );

        Dispatcher.UIThread.Invoke(() =>
        {
            var selectedId = SelectedPlugin.Value?.Id;
            SelectedPlugin.OnNext(null);
            _plugins.RemoveAll();
            var filtered = IsShowOnlyVerified.ViewValue.Value
                ? items.Where(x => x.IsVerified)
                : items;
            _plugins.AddRange(filtered);

            var first = _view.FirstOrDefault(x => x.Id == selectedId);
            SelectedPlugin.OnNext(first);
        });
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        foreach (var viewModel in _view)
        {
            yield return viewModel;
        }

        yield return Search;
        yield return IsShowOnlyVerified;
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => PluginManagerModule.Instance;
}

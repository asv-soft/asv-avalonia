using System.Composition;
using Asv.Cfg;
using Asv.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class SettingsHotKeysListViewModelConfig
{
    public string SearchText { get; set; } = string.Empty;
    public string SelectedItemId { get; set; } = string.Empty;
}

[ExportSettings(PageId)]
public class SettingsHotKeysListViewModel : SettingsSubPage
{
    public const string PageId = "hotkeys";

    private readonly ICommandService _commandsService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IDialogService _dialogService;
    private readonly ISearchService _searchService;
    private readonly ObservableList<ICommandInfo> _itemsSource;
    private readonly ILayoutService _layoutService;
    private readonly ISynchronizedView<ICommandInfo, HotKeyViewModel> _view;

    private SettingsHotKeysListViewModelConfig _config;

    public SettingsHotKeysListViewModel()
        : this(
            DesignTime.CommandService,
            DesignTime.LoggerFactory,
            NullDialogService.Instance,
            NullLayoutService.Instance,
            NullSearchService.Instance
        )
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    [ImportingConstructor]
    public SettingsHotKeysListViewModel(
        ICommandService commandsService,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        ILayoutService layoutService,
        ISearchService searchService
    )
        : base(PageId, loggerFactory)
    {
        _commandsService = commandsService;
        _loggerFactory = loggerFactory;
        _dialogService = dialogService;
        _searchService = searchService;
        _layoutService = layoutService;

        SelectedItem = new BindableReactiveProperty<HotKeyViewModel?>().DisposeItWith(Disposable);

        Search = new SearchBoxViewModel(
            nameof(Search),
            loggerFactory,
            UpdateImpl,
            TimeSpan.FromMilliseconds(500)
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        _itemsSource = new ObservableList<ICommandInfo>(
            commandsService.Commands.Where(x => x.DefaultHotKey is not null)
        );

        _view = _itemsSource
            .CreateView(cmdInfo => new HotKeyViewModel(
                cmdInfo,
                _commandsService,
                _dialogService,
                _loggerFactory
            ))
            .DisposeItWith(Disposable);
        _view.SetRoutableParent(this).DisposeItWith(Disposable);

        Items = _view.ToNotifyCollectionChanged().DisposeItWith(Disposable);

        Search.Refresh();
    }

    public SearchBoxViewModel Search { get; }
    public BindableReactiveProperty<HotKeyViewModel?> SelectedItem { get; }
    public INotifyCollectionChangedSynchronizedViewList<HotKeyViewModel> Items { get; }

    private Task UpdateImpl(string? query, IProgress<double> progress, CancellationToken cancel)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            _view.ResetFilter();
            _view.ForEach(vm => vm.ResetSelections());
        }
        else
        {
            _view.AttachFilter(
                new SynchronizedViewFilter<ICommandInfo, HotKeyViewModel>(
                    (_, model) => model.Filter(query, _searchService)
                )
            );
        }

        progress.Report(1);
        return Task.CompletedTask;
    }

    public void ResetAllHotKeys() // TODO: Make a command
    {
        _commandsService.ResetAllHotKeys();

        Search.Refresh();

        Logger.LogInformation("All hot keys have been reset to default.");
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return Search;
        foreach (var item in _view)
        {
            yield return item;
        }

        foreach (var children in base.GetRoutableChildren())
        {
            yield return children;
        }
    }

    protected override ValueTask HandleSubpageSave()
    {
        _config.SearchText = Search.Text.ViewValue.Value ?? string.Empty;
        _config.SelectedItemId = SelectedItem.Value?.Id.ToString() ?? string.Empty;
        _layoutService.Set(this, _config);
        return base.HandleSubpageSave();
    }

    protected override ValueTask HandleSubpageLoad()
    {
        _config = _layoutService.Get(this, new Lazy<SettingsHotKeysListViewModelConfig>());
        Search.Text.ModelValue.Value = _config.SearchText;
        var selected = _view.FirstOrDefault(x => x.Id.ToString() == _config.SelectedItemId);

        if (selected is not null)
        {
            SelectedItem.Value = selected;
        }

        Search.Text.ViewValue.SubscribeSaveState(this).DisposeItWith(Disposable);
        SelectedItem.SubscribeSaveState(this).DisposeItWith(Disposable);
        return base.HandleSubpageLoad();
    }

    public override IExportInfo Source => SystemModule.Instance;
}

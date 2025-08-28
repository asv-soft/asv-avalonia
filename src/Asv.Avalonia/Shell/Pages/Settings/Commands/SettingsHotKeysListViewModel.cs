using System.Composition;
using Asv.Cfg;
using Asv.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class SettingsHotKeysListViewModelConfig : TreeSubpageConfig
{
    public string SearchText { get; set; } = string.Empty;
    public string SelectedCommandId { get; set; } = string.Empty;
}

[ExportSettings(PageId)]
public class SettingsHotKeysListViewModel : SettingsSubPage<SettingsHotKeysListViewModelConfig>
{
    public const string PageId = "hotkeys";

    private readonly ICommandService _commandsService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IDialogService _dialogService;
    private readonly ISearchService _searchService;
    private readonly IStateSaver<SettingsHotKeysListViewModelConfig> _stateSaver;
    private readonly ObservableList<ICommandInfo> _itemsSource;
    private readonly ISynchronizedView<ICommandInfo, HotKeyViewModel> _view;

    public SettingsHotKeysListViewModel()
        : this(
            DesignTime.CommandService,
            DesignTime.LoggerFactory,
            NullDialogService.Instance,
            DesignTime.Configuration,
            NullStateSaverFactory.Instance,
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
        IConfiguration cfg,
        IStateSaverFactory stateSaverFactory,
        ISearchService searchService
    )
        : base(PageId, cfg, loggerFactory)
    {
        _commandsService = commandsService;
        _loggerFactory = loggerFactory;
        _dialogService = dialogService;
        _searchService = searchService;
        _stateSaver = stateSaverFactory.Create<SettingsHotKeysListViewModelConfig>();

        SelectedItem = new BindableReactiveProperty<HotKeyViewModel?>().DisposeItWith(Disposable);

        Search = new SearchBoxViewModel(
            nameof(Search),
            loggerFactory,
            UpdateImpl,
            TimeSpan.FromMilliseconds(500)
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        _stateSaver
            .Add(Search.Text, (value, cf) => cf.SearchText = value)
            .DisposeItWith(Disposable);
        _stateSaver
            .Add(
                SelectedItem,
                (value, cf) => cf.SelectedCommandId = value?.Id.ToString() ?? string.Empty
            )
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

        var selectedItem = _view.FirstOrDefault(c => c.Id == _stateSaver.Config.SelectedCommandId);
        if (selectedItem is not null)
        {
            SelectedItem.OnNext(selectedItem);
        }

        Search.Text.Value = _stateSaver.Config.SearchText;
        Search.Refresh();

        var obsv = Observable.Merge(
            Search.Text.Skip(1).Select(_ => Unit.Default),
            SelectedItem.Skip(1).Select(_ => Unit.Default)
        );
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

    // public override ValueTask SaveChanges(CancellationToken cancellationToken)
    // {
    //     Config.SelectedCommandId = SelectedItem.Value?.Id.ToString() ?? string.Empty;
    //     Config.SearchText = Search.Text.Value;
    //     return base.SaveChanges(cancellationToken);
    // }
    public override IExportInfo Source => SystemModule.Instance;
}

using System.Composition;
using Asv.Cfg;
using Asv.Common;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

[ExportSettings(PageId)]
public class SettingsHotKeysListViewModel : SettingsSubPage
{
    public const string PageId = "hotkeys";

    private readonly ICommandService _commandsService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IDialogService _dialogService;
    private readonly ISearchService _searchService;
    private readonly ObservableList<ICommandInfo> _itemsSource;
    private readonly ISynchronizedView<ICommandInfo, HotKeyViewModel> _view;

    public SettingsHotKeysListViewModel()
        : this(
            DesignTime.CommandService,
            DesignTime.LoggerFactory,
            NullDialogService.Instance,
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
        ISearchService searchService
    )
        : base(PageId, loggerFactory)
    {
        _commandsService = commandsService;
        _loggerFactory = loggerFactory;
        _dialogService = dialogService;
        _searchService = searchService;

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

    public override IExportInfo Source => SystemModule.Instance;
}

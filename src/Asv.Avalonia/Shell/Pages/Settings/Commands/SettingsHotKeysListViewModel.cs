using System.Composition;
using Asv.Cfg;
using Asv.Common;
using Avalonia.Input;
using Avalonia.Threading;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using ZLogger;

namespace Asv.Avalonia;

public class SettingsHotKeysListViewModelConfig : TreeSubpageConfig { }

[ExportSettings(PageId)]
public class SettingsHotKeysListViewModel : SettingsSubPage<SettingsHotKeysListViewModelConfig>
{
    public const string PageId = "hotkeys";

    private readonly ICommandService _commandsService;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IDialogService _dialogService;
    private readonly ISearchService _searchService;
    private readonly ObservableList<HotKeyViewModel> _itemsSource;

    #region Design

    public SettingsHotKeysListViewModel()
        : this(
            DesignTime.CommandService,
            DesignTime.LoggerFactory,
            NullDialogService.Instance,
            DesignTime.Configuration,
            NullSearchService.Instance
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        _itemsSource = new ObservableList<HotKeyViewModel>(
            new List<ICommandInfo>(
                [
                    new CommandInfo
                    {
                        Id = "command1",
                        Name = "Command1",
                        Description = "Description for Command1",
                        Icon = MaterialIconKind.Abacus,
                        DefaultHotKey = new HotKeyInfo(KeyGesture.Parse("Ctrl+Shift+A")),
                        Source = SystemModule.Instance,
                    },
                    new CommandInfo
                    {
                        Id = "command2",
                        Name = "Command2",
                        Description = "Description for Command2",
                        Icon = MaterialIconKind.ABCOff,
                        DefaultHotKey = new HotKeyInfo(KeyGesture.Parse("Ctrl+F")),
                        Source = SystemModule.Instance,
                    },
                    new CommandInfo
                    {
                        Id = "command3",
                        Name = "Command3",
                        Description = "Description for Command3",
                        Icon = MaterialIconKind.AbTesting,
                        DefaultHotKey = null,
                        Source = SystemModule.Instance,
                    },
                ]
            ).Select(x => new HotKeyViewModel(
                this,
                x,
                _commandsService,
                _dialogService,
                _loggerFactory
            ))
        );
        Items = _itemsSource
            .ToNotifyCollectionChangedSlim()
            .SetRoutableParent(this, Disposable)
            .DisposeItWith(Disposable);

        Search = new SearchBoxViewModel();
    }

    #endregion

    [ImportingConstructor]
    public SettingsHotKeysListViewModel(
        ICommandService commandsService,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IConfiguration cfg,
        ISearchService searchService
    )
        : base(PageId, cfg, loggerFactory)
    {
        _commandsService = commandsService;
        _loggerFactory = loggerFactory;
        _dialogService = dialogService;
        _searchService = searchService;

        Search = new SearchBoxViewModel(
            nameof(Search),
            loggerFactory,
            UpdateImpl,
            TimeSpan.FromMilliseconds(500)
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        _itemsSource = new ObservableList<HotKeyViewModel>(
            commandsService
                .Commands.Where(x => x.DefaultHotKey is not null)
                .Select(x => new HotKeyViewModel(
                    this,
                    x,
                    _commandsService,
                    _dialogService,
                    _loggerFactory
                ))
        );
        Items = _itemsSource
            .ToNotifyCollectionChangedSlim()
            .SetRoutableParent(this, Disposable)
            .DisposeItWith(Disposable);

        Search.Refresh();
    }

    public SearchBoxViewModel Search { get; }
    public HotKeyViewModel SelectedItem
    {
        get;
        set => SetField(ref field, value);
    }
    public INotifyCollectionChangedSynchronizedViewList<HotKeyViewModel> Items { get; }

    private async Task UpdateImpl( // TODO: Simplify with the common filtering from ObservableCollections
        string? query,
        IProgress<double> progress,
        CancellationToken cancel
    )
    {
        try
        {
            var text = query?.ToLower();
            await Dispatcher.UIThread.InvokeAsync(_itemsSource.Clear);
            var editableCommands = _commandsService
                .Commands.Where(x => x.DefaultHotKey is not null)
                .ToList();
            var filtered = 0;
            var total = editableCommands.Count;
            Logger.ZLogTrace($"Start filtering log messages with filter: '{text}'");
            progress.Report(double.NaN);

            foreach (
                var command in editableCommands.TakeWhile(_ => !cancel.IsCancellationRequested)
            )
            {
                ++filtered;

                if (
                    HotKeyViewModel.TryCreate(
                        command,
                        this,
                        _searchService,
                        _commandsService,
                        _dialogService,
                        query,
                        _loggerFactory,
                        out var vm
                    )
                    && vm != null
                )
                {
                    ++filtered;
                    await Dispatcher.UIThread.InvokeAsync(() => _itemsSource.Add(vm));
                    progress.Report((double)total / filtered);
                    Logger.ZLogTrace($"Filtered {filtered} items from {total}");
                }
            }
        }
        finally
        {
            _itemsSource.Sort(HotKeyViewModelComparer.Instance);
            progress.Report(1);
        }
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
        foreach (var item in Items)
        {
            yield return item;
        }
    }

    public override IExportInfo Source => SystemModule.Instance;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SelectedItem.Dispose();
        }

        base.Dispose(disposing);
    }
}

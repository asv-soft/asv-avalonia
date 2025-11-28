using System.Composition;
using Asv.Common;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public sealed class SettingsCommandListViewModelConfig
{
    public string SearchText { get; set; } = string.Empty;
    public string SelectedItemId { get; set; } = string.Empty;
    public CommandSortingType CommandSortingType { get; set; } = CommandSortingType.All;
}

[ExportSettings(PageId)]
public class SettingsCommandListViewModel : SettingsSubPage
{
    public const string PageId = "hotkeys";

    private readonly ICommandService _commandsService;
    private readonly ObservableList<ICommandInfo> _itemsSource;
    private readonly ISynchronizedView<ICommandInfo, CommandViewModel> _view;
    private readonly ReactiveProperty<Enum> _commandSortingType;

    private SettingsCommandListViewModelConfig? _config;

    public SettingsCommandListViewModel()
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
    public SettingsCommandListViewModel(
        ICommandService commandsService,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        ISearchService searchService
    )
        : base(PageId, loggerFactory)
    {
        _commandsService = commandsService;

        SelectedItem = new BindableReactiveProperty<CommandViewModel?>();

        _itemsSource = new ObservableList<ICommandInfo>(commandsService.Commands);
        _view = _itemsSource
            .CreateView(cmdInfo => new CommandViewModel(
                cmdInfo,
                commandsService,
                searchService,
                dialogService,
                loggerFactory
            ))
            .DisposeItWith(Disposable);
        _view.DisposeMany().DisposeItWith(Disposable);
        _view.SetRoutableParent(this).DisposeItWith(Disposable);
        Items = _view
            .ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);

        Search = new SearchBoxViewModel(
            nameof(Search),
            loggerFactory,
            UpdateImpl,
            TimeSpan.FromMilliseconds(500)
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        _commandSortingType = new ReactiveProperty<Enum>(
            Avalonia.CommandSortingType.All
        ).DisposeItWith(Disposable);
        CommandSortingType = new HistoricalEnumProperty<CommandSortingType>(
            nameof(CommandSortingType),
            _commandSortingType,
            loggerFactory
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        CommandSortingType.ViewValue.Subscribe(_ => Search.Refresh()).DisposeItWith(Disposable);

        SelectedItem
            .ObserveOnUIThreadDispatcher()
            .Skip(1)
            .Subscribe(selectedCommand =>
            {
                foreach (var commandViewModel in _view)
                {
                    if (commandViewModel.Id == selectedCommand?.Id)
                    {
                        commandViewModel.IsSelected.Value = true;
                        continue;
                    }

                    commandViewModel.IsSelected.Value = false;
                }
            })
            .DisposeItWith(Disposable);
        Search.Refresh();
    }

    public SearchBoxViewModel Search { get; }
    public BindableReactiveProperty<CommandViewModel?> SelectedItem { get; }

    public INotifyCollectionChangedSynchronizedViewList<CommandViewModel> Items { get; }
    public HistoricalEnumProperty<CommandSortingType> CommandSortingType { get; }

    private Task UpdateImpl(string? query, IProgress<double> progress, CancellationToken cancel)
    {
        progress.Report(0);
        var isSearchTextEmpty = string.IsNullOrWhiteSpace(Search.Text.ViewValue.CurrentValue);
        var isCommandSortEmpty =
            CommandSortingType.ViewValue.CurrentValue is Avalonia.CommandSortingType.All;

        if (isSearchTextEmpty && isCommandSortEmpty)
        {
            _view.ForEach(vm => vm.Filter(Search.Text.ViewValue.CurrentValue ?? string.Empty));
            _view.ResetFilter();
            progress.Report(1);
            return Task.CompletedTask;
        }

        var filter = new SynchronizedViewFilter<ICommandInfo, CommandViewModel>(
            (_, model) =>
            {
                var isOk = true;

                if (!string.IsNullOrWhiteSpace(Search.Text.ViewValue.CurrentValue))
                {
                    isOk = isOk && model.Filter(Search.Text.ViewValue.CurrentValue);
                }

                return isOk
                    && CommandSortingType.ViewValue.CurrentValue switch
                    {
                        Avalonia.CommandSortingType.All => true,
                        Avalonia.CommandSortingType.WithHotkeysOnly => model.DefaultHotKey
                            is not null,
                        Avalonia.CommandSortingType.WithoutHotkeysOnly => model.DefaultHotKey
                            is null,
                        _ => false,
                    };
            }
        );

        _view.AttachFilter(filter);

        progress.Report(1);
        return Task.CompletedTask;
    }

    public void ResetAllHotKeys() // TODO: Make a command
    {
        _commandsService.ResetAllHotKeys();

        Dispatcher.UIThread.Invoke(() =>
        {
            _itemsSource.RemoveAll();
            _itemsSource.AddRange(_commandsService.Commands);
            Search.Refresh();
        });

        Logger.LogInformation("All hot keys have been reset to default");
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return Search;
        yield return CommandSortingType;

        foreach (var item in _view)
        {
            yield return item;
        }

        foreach (var children in base.GetRoutableChildren())
        {
            yield return children;
        }
    }

    protected override ValueTask InternalCatchEvent(AsyncRoutedEvent e)
    {
        switch (e)
        {
            case SaveLayoutEvent saveLayoutEvent:
                if (_config is null)
                {
                    break;
                }

                this.HandleSaveLayout(
                    saveLayoutEvent,
                    _config,
                    cfg =>
                    {
                        cfg.SearchText = Search.Text.ViewValue.Value ?? string.Empty;
                        cfg.CommandSortingType = CommandSortingType.ViewValue.Value;
                        cfg.SelectedItemId = SelectedItem.Value?.Id.ToString() ?? string.Empty;
                    }
                );
                break;
            case LoadLayoutEvent loadLayoutEvent:
                _config = this.HandleLoadLayout<SettingsCommandListViewModelConfig>(
                    loadLayoutEvent,
                    cfg =>
                    {
                        Search.Text.ModelValue.Value = cfg.SearchText;
                        CommandSortingType.ModelValue.Value = cfg.CommandSortingType;
                        SelectedItem.Value = _view.FirstOrDefault(x =>
                            x.Id.ToString() == cfg.SelectedItemId
                        );
                    }
                );
                break;
        }

        return base.InternalCatchEvent(e);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SelectedItem.Value = null;
            SelectedItem.Dispose();
        }

        base.Dispose(disposing);
    }

    public override IExportInfo Source => SystemModule.Instance;
}

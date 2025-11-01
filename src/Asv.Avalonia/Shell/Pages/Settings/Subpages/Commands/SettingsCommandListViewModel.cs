using System.Composition;
using Asv.Common;
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
    private readonly ISearchService _searchService;
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
        _searchService = searchService;

        SelectedItem = new BindableReactiveProperty<CommandViewModel?>().DisposeItWith(Disposable);

        _itemsSource = new ObservableList<ICommandInfo>(commandsService.Commands);
        _view = _itemsSource
            .CreateView(cmdInfo => new CommandViewModel(
                cmdInfo,
                commandsService,
                dialogService,
                loggerFactory
            ))
            .DisposeItWith(Disposable);
        _view.SetRoutableParent(this).DisposeItWith(Disposable);
        Items = _view.ToNotifyCollectionChanged().DisposeItWith(Disposable);

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
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        CommandSortingType.ViewValue.Subscribe(_ => UpdateFilter()).DisposeItWith(Disposable);

        Search.Refresh();
    }

    public SearchBoxViewModel Search { get; }
    public BindableReactiveProperty<CommandViewModel?> SelectedItem { get; }
    public INotifyCollectionChangedSynchronizedViewList<CommandViewModel> Items { get; }
    public HistoricalEnumProperty<CommandSortingType> CommandSortingType { get; }

    private Task UpdateImpl(string? query, IProgress<double> progress, CancellationToken cancel)
    {
        UpdateFilter();

        progress.Report(1);
        return Task.CompletedTask;
    }

    private void UpdateFilter()
    {
        _view.ForEach(vm => vm.ResetSelections());

        var isSearchTextEmpty = string.IsNullOrWhiteSpace(Search.Text.ViewValue.CurrentValue);
        var isCommandSortEmpty =
            CommandSortingType.ViewValue.CurrentValue is Avalonia.CommandSortingType.All;

        if (isSearchTextEmpty && isCommandSortEmpty)
        {
            _view.ResetFilter();
            return;
        }

        var filter = new SynchronizedViewFilter<ICommandInfo, CommandViewModel>(
            (_, model) =>
            {
                var isOk = true;

                if (!string.IsNullOrWhiteSpace(Search.Text.ViewValue.CurrentValue))
                {
                    isOk = isOk && model.Filter(Search.Text.ViewValue.CurrentValue, _searchService);
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
    }

    public void ResetAllHotKeys() // TODO: Make a command
    {
        _commandsService.ResetAllHotKeys();

        _itemsSource.Clear();
        _itemsSource.AddRange(_commandsService.Commands);

        Search.Refresh();

        Logger.LogInformation("All hot keys have been reset to default.");
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

    public override IExportInfo Source => SystemModule.Instance;
}

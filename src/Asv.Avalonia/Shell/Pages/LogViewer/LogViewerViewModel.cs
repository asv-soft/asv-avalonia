using System.Composition;
using Asv.Avalonia.Comparers;
using Asv.Common;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Data;
using Material.Icons;
using Material.Icons.Avalonia;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

[ExportPage(PageId)]
public class LogViewerViewModel : PageViewModel<LogViewerViewModel>
{
    public const string PageId = "log_viewer";
    private const int MinPageIndex = 1;

    private readonly ILogService _log;

    private readonly HashSet<LogLevel> _logLevels;
    private readonly HashSet<string> _logCategories;
    private readonly HashSet<string> _logThreadIds;

    private readonly ObservableList<LogItemViewModel> _logItems;

    public LogViewerViewModel()
        : this(DesignTime.CommandService, DesignTime.LogService) { }

    [ImportingConstructor]
    public LogViewerViewModel(ICommandService cmd, ILogService log)
        : base(PageId, cmd)
    {
        _log = log;

        Icon.OnNext(MaterialIconKind.Journal);
        Title.OnNext(RS.LogViewer_Title);

        _logLevels = new HashSet<LogLevel>();
        _logCategories = new HashSet<string>();
        _logThreadIds = new HashSet<string>();

        _logItems = new ObservableList<LogItemViewModel>();

        using (var logView = _logItems.CreateView(x => x))
        {
            logView.AttachFilter(FilterItem);
            _logItems.Sort(new LogItemDescendingComparer());
            LogItemsView = logView.ToNotifyCollectionChanged();
        }

        _log.LoadItemsFromLogFile()
            .ForEach(x => _logItems.Add(new LogItemViewModel(x.Timestamp.Ticks, x)));

        _logItems.Select(_ => _.Level).ForEach(level => _logLevels.Add(level));
        _logItems.Select(_ => _.Category).ForEach(category => _logCategories.Add(category));

        PageSize = new BindableReactiveProperty<int>(PageSizes[0]);
        FilteredItemsCount = new BindableReactiveProperty<int>();

        TotalItemsCount = new BindableReactiveProperty<int>(_logItems.Count);
        TotalPagesCount = new BindableReactiveProperty<int>(
            (int)Math.Ceiling((double)TotalItemsCount.Value / PageSize.Value)
        );

        SelectedLevels = new ObservableList<LogLevel>()
            .CreateWritableView(x => x)
            .ToWritableNotifyCollectionChanged();
        SelectedCategories = new ObservableList<string>()
            .CreateWritableView(x => x)
            .ToWritableNotifyCollectionChanged();
        SelectedThreadIds = new ObservableList<string>()
            .CreateWritableView(x => x)
            .ToWritableNotifyCollectionChanged();

        AvailableLevels.ForEach(level => SelectedLevels.Add(level));
        AvailableCategories.ForEach(cls => SelectedCategories.Add(cls));
        AvailableThreadIds.ForEach(thread => SelectedThreadIds.Add(thread));

        SearchText = new BindableReactiveProperty<string>();
        CurrentPageIndex = new BindableReactiveProperty<int>();

        SearchText.Subscribe(_ => CurrentPageIndex.OnNext(1));

        Observable
            .CombineLatest(PageSize, FilteredItemsCount)
            .Subscribe(tuple =>
            {
                var pageSize = tuple[0];
                var filteredCount = tuple[1];

                TotalPagesCount.OnNext((int)Math.Ceiling((double)filteredCount / pageSize));
                if (CurrentPageIndex.Value > TotalPagesCount.Value)
                {
                    CurrentPageIndex.OnNext(TotalPagesCount.Value);
                }

                if (CurrentPageIndex.Value < MinPageIndex)
                {
                    CurrentPageIndex.OnNext(MinPageIndex);
                }
            });

        MoveToPreviousPageCommand = new ReactiveCommand(_ => MoveToPreviousPage());
        Observable
            .CombineLatest(CurrentPageIndex, TotalPagesCount)
            .Subscribe(x => MoveToPreviousPageCommand.ChangeCanExecute(x[0] > MinPageIndex));

        MoveToNextPageCommand = new ReactiveCommand(_ => MoveToNextPage());
        Observable
            .CombineLatest(CurrentPageIndex, TotalPagesCount)
            .Subscribe(x => MoveToPreviousPageCommand.ChangeCanExecute(x[0] > x[1]));

        ClearSearchTextCommand = new ReactiveCommand(_ => SearchText.OnNext(string.Empty));
        SearchText.Subscribe(text =>
            ClearSearchTextCommand.ChangeCanExecute(!string.IsNullOrEmpty(text))
        );

        SelectAllThreadIdsCommand = new ReactiveCommand(_ =>
            AvailableThreadIds.ForEach(thread => SelectedThreadIds.Add(thread))
        );
        DeselectAllThreadIdsCommand = new ReactiveCommand(_ => SelectedThreadIds.Clear());

        SelectAllCategoriesCommand = new ReactiveCommand(_ =>
            AvailableCategories.ForEach(cls => SelectedCategories.Add(cls))
        );
        DeselectAllCategoriesCommand = new ReactiveCommand(_ => SelectedCategories.Clear());

        SelectAllLevelsCommand = new ReactiveCommand(_ =>
            AvailableLevels.ForEach(level => SelectedLevels.Add(level))
        );
        DeselectAllLevelsCommand = new ReactiveCommand(_ => SelectedLevels.Clear());

        ClearLogsCommand = new ReactiveCommand(ClearLogs);
    }

    public static List<int> PageSizes => [25, 50, 100, 200];

    public IEnumerable<LogLevel> AvailableLevels => _logLevels;
    public IEnumerable<string> AvailableCategories => _logCategories;
    public IEnumerable<string> AvailableThreadIds => _logThreadIds;

    public BindableReactiveProperty<int> TotalItemsCount { get; set; }
    public BindableReactiveProperty<int> FilteredItemsCount { get; set; }
    public BindableReactiveProperty<string> SearchText { get; set; }
    public BindableReactiveProperty<int> CurrentPageIndex { get; set; }
    public BindableReactiveProperty<int> TotalPagesCount { get; set; }
    public BindableReactiveProperty<int> PageSize { get; set; }
    public BindableReactiveProperty<SortDirection> SortDirectionType { get; set; }

    public NotifyCollectionChangedSynchronizedViewList<LogItemViewModel> LogItemsView { get; set; }

    public NotifyCollectionChangedSynchronizedViewList<LogLevel> SelectedLevels { get; set; }
    public NotifyCollectionChangedSynchronizedViewList<string> SelectedCategories { get; set; }
    public NotifyCollectionChangedSynchronizedViewList<string> SelectedThreadIds { get; set; }

    public ReactiveCommand SelectAllThreadIdsCommand { get; }
    public ReactiveCommand DeselectAllThreadIdsCommand { get; }
    public ReactiveCommand SelectAllCategoriesCommand { get; }
    public ReactiveCommand DeselectAllCategoriesCommand { get; }
    public ReactiveCommand SelectAllLevelsCommand { get; }
    public ReactiveCommand DeselectAllLevelsCommand { get; }
    public ReactiveCommand MoveToPreviousPageCommand { get; }
    public ReactiveCommand MoveToNextPageCommand { get; }
    public ReactiveCommand ClearSearchTextCommand { get; }
    public ReactiveCommand ClearLogsCommand { get; }

    private bool FilterItem(LogItemViewModel item)
    {
        var containsMessage =
            string.IsNullOrEmpty(SearchText.Value)
            || item.Message.Contains(SearchText.Value, StringComparison.CurrentCultureIgnoreCase);
        var containsLevel = SelectedLevels.Contains(item.Level);
        var containsClass = SelectedCategories.Contains(item.Category);

        return containsMessage && containsLevel && containsClass;
    }

    private void MoveToPreviousPage()
    {
        CurrentPageIndex.OnNext(CurrentPageIndex.Value - 1);
    }

    private void MoveToNextPage()
    {
        CurrentPageIndex.OnNext(CurrentPageIndex.Value + 1);
    }

    private async ValueTask ClearLogs(Unit unit, CancellationToken ct)
    {
        var titlePanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Children =
            {
                new MaterialIcon
                {
                    Foreground = new SolidColorBrush(Colors.Yellow),
                    Height = 20,
                    Width = 20,
                    Kind = MaterialIconKind.Warning,
                },
                new TextBlock { Text = RS.LogViewerViewModel_ClearDialog_Title },
            },
        };

        var confirmDialog = new ContentDialog
        {
            Title = titlePanel,
            Content = RS.LogViewerViewModel_ClearDialog_Content,
            PrimaryButtonText = RS.LogViewerViewModel_ClearDialog_PrimaryButton,
            CloseButtonText = RS.LogViewerViewModel_ClearDialog_CloseButton,
        };

        var result = await confirmDialog.ShowAsync();

        if (result is ContentDialogResult.Primary)
        {
            _logItems.Clear();
            _log.DeleteLogFile();
            _log.Warning(nameof(LogViewerViewModel), "Log have been cleared!");
        }
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        return ValueTask.FromResult<IRoutable>(this);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    protected override void AfterLoadExtensions() { }
}

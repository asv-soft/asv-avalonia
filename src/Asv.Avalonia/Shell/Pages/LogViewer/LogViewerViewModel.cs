using System.Diagnostics;
using Asv.Cfg;
using Asv.Common;
using Asv.Modeling;
using Avalonia.Threading;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class LogViewerViewModelConfig
{
    public string SearchText { get; set; } = string.Empty;
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 50;
}

public interface ILogViewerViewModel : IPage { }

public class LogViewerViewModel
    : PageViewModel<ILogViewerViewModel>,
        ILogViewerViewModel,
        ISupportPagination
{
    public const string PageId = "log";
    public const MaterialIconKind PageIcon = MaterialIconKind.Journal;
    public const AsvColorKind PageIconColor = AsvColorKind.None;

    private readonly ILogReaderService _logReaderService;
    private readonly ISearchService _search;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ObservableList<LogMessageViewModel> _itemsSource = new();

    public LogViewerViewModel()
        : base(
            DesignTime.Id.TypeId,
            DesignTime.PageContext,
            DesignTime.LoggerFactory,
            DesignTime.DialogService,
            DesignTime.ExtensionService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        Header = RS.LogViewerViewModel_Title;
        Icon = PageIcon;
        IconColor = PageIconColor;
        Search = new SearchBoxViewModel();
        Items = _itemsSource
            .ToNotifyCollectionChangedSlim(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);

        _itemsSource.Add(
            new LogMessageViewModel(
                new LogMessage(
                    DateTime.Now,
                    LogLevel.Error,
                    "DesignTime",
                    "Design time log message",
                    "This is a design time log message for the Log Viewer. It will not be shown in the actual application."
                ),
                this
            )
        );
        _itemsSource.Add(
            new LogMessageViewModel(
                new LogMessage(
                    DateTime.Now,
                    LogLevel.Information,
                    "asd.asd.asd.a.dsasd",
                    "Design time log message 2",
                    "This is another design time log message for the Log Viewer. It will not be shown in the actual application."
                ),
                this
            )
        );
        _itemsSource.Add(
            new LogMessageViewModel(
                new LogMessage(
                    DateTime.Now,
                    LogLevel.Warning,
                    "DesignTime",
                    "Design time log message 3",
                    "This is yet another design time log message for the Log Viewer. It will not be shown in the actual application."
                ),
                this
            )
        );
        _itemsSource.Add(
            new LogMessageViewModel(
                new LogMessage(
                    DateTime.Now,
                    LogLevel.Debug,
                    "asdasdasdasdasd",
                    "Design time log message 4",
                    "This is a debug log message for the Log Viewer. It will not be shown in the actual application."
                ),
                this
            )
        );
        _itemsSource.Add(
            new LogMessageViewModel(
                new LogMessage(
                    DateTime.Now,
                    LogLevel.Critical,
                    "DesignTime",
                    "Design time log message 5",
                    "This is a critical log message for the Log Viewer. It will not be shown in the actual application."
                ),
                this
            )
        );
        _itemsSource.Add(
            new LogMessageViewModel(
                new LogMessage(
                    DateTime.Now,
                    LogLevel.Trace,
                    "asdasdasd",
                    "Design time log message 6",
                    "This is a trace log message for the Log Viewer. It will not be shown in the actual application."
                ),
                this
            )
        );
        _itemsSource.Add(
            new LogMessageViewModel(
                new LogMessage(
                    DateTime.Now,
                    LogLevel.None,
                    "DesignTime asdasd",
                    "Design time log message 7",
                    "This is a log message with no specific level for the Log Viewer. It will not be shown in the actual application."
                ),
                this
            )
        );
    }

    public LogViewerViewModel(
        IPageContext context,
        ILogReaderService logReaderService,
        ISearchService search,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IConfiguration cfg,
        IExtensionService ext
    )
        : base(PageId, context, loggerFactory, dialogService, ext)
    {
        _logReaderService = logReaderService;
        _search = search;
        _loggerFactory = loggerFactory;
        Header = RS.LogViewerViewModel_Title;
        Icon = PageIcon;
        IconColor = PageIconColor;
        Items = _itemsSource
            .ToNotifyCollectionChangedSlim()
            .SetRoutableParent(this, Disposable)
            .DisposeItWith(Disposable);

        Skip = new BindableReactiveProperty<int>(0).DisposeItWith(Disposable);
        Take = new BindableReactiveProperty<int>(50).DisposeItWith(Disposable);
        FromToText = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(Disposable);
        TextMessage = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(Disposable);

        Search = new SearchBoxViewModel(
            nameof(Search),
            loggerFactory,
            UpdateImpl,
            TimeSpan.FromMilliseconds(500)
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        Next = new ReactiveCommand(_ => Skip.Value += Take.Value).DisposeItWith(Disposable);
        Previous = new ReactiveCommand(_ =>
            Skip.Value = Math.Max(0, Skip.Value - Take.Value)
        ).DisposeItWith(Disposable);
        Skip.Merge(Take).Subscribe(_ => Search.Refresh()).DisposeItWith(Disposable);
        Search.Refresh();
    }

    public SearchBoxViewModel Search { get; }

    public BindableReactiveProperty<int> Skip { get; }
    public BindableReactiveProperty<int> Take { get; }

    public INotifyCollectionChangedSynchronizedViewList<LogMessageViewModel> Items { get; }

    public BindableReactiveProperty<string> FromToText { get; }
    public BindableReactiveProperty<string> TextMessage { get; }

    public ReactiveCommand Next { get; }
    public ReactiveCommand Previous { get; }

    public LogMessageViewModel SelectedItem
    {
        get;
        set => SetField(ref field, value);
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Search;
        foreach (var item in Items)
        {
            yield return item;
        }
    }

    protected override void AfterLoadExtensions()
    {
        var trigger = Search
            .Text.ViewValue.Select(_ => Unit.Default)
            .Merge(Skip.Select(_ => Unit.Default))
            .Merge(Take.Select(_ => Unit.Default));

        Layout
            .Register(nameof(LogViewerViewModel), LoadLayout, SaveLayout, trigger)
            .DisposeItWith(Disposable);
        Layout.LoadWhenRootAttached(RootTracking).AddTo(ref DisposableBag);
    }

    private async Task UpdateImpl(
        string? query,
        IProgress<double> progress,
        CancellationToken cancel
    )
    {
        var sw = new Stopwatch();
        sw.Start();
        try
        {
            using var veryFastMessageProperty = new ReactiveProperty<string>();

            // we use it here to avoid UI thread blocking
            using var subscription = veryFastMessageProperty
                .ThrottleFirstFrame(1)
                .Subscribe(TextMessage.AsObserver());
            var text = query?.ToLower();
            await Dispatcher.UIThread.InvokeAsync(_itemsSource.Clear);
            var filtered = 0;
            var total = 0;
            var skip = 0;

            progress.Report(double.NaN);

            await foreach (
                var logMessage in _logReaderService
                    .LoadItemsFromLogFile(cancel)
                    .ConfigureAwait(false)
            )
            {
                if (cancel.IsCancellationRequested)
                {
                    break;
                }

                ++total;

                if (
                    LogMessageViewModel.TryCreate(
                        logMessage,
                        _search,
                        text,
                        _loggerFactory,
                        this,
                        out var vm
                    )
                    && vm != null
                )
                {
                    ++filtered;
                    if (filtered < Skip.Value)
                    {
                        ++skip;
                        veryFastMessageProperty.OnNext(
                            string.Format(
                                RS.LogViewerViewModel_Pagination_SkippedFiltered,
                                skip,
                                filtered,
                                total
                            )
                        );
                    }
                    else
                    {
                        await Dispatcher.UIThread.InvokeAsync(() => _itemsSource.Add(vm));
                        progress.Report((double)_itemsSource.Count / Take.Value);
                        veryFastMessageProperty.OnNext(
                            string.Format(
                                RS.LogViewerViewModel_Pagination_SkippedFiltered,
                                skip,
                                filtered,
                                total
                            )
                        );
                    }

                    if (_itemsSource.Count >= Take.Value)
                    {
                        break;
                    }
                }
                else
                {
                    // this is for performance reasons, we skip the messages that do not match the filter
                    if (total % 100 == 0)
                    {
                        veryFastMessageProperty.OnNext(
                            string.Format(
                                RS.LogViewerViewModel_Pagination_SkippedFiltered,
                                skip,
                                filtered,
                                total
                            )
                        );
                    }
                }
            }

            TextMessage.Value = string.Format(
                RS.LogViewerViewModel_Pagination_SkippedFilteredBy,
                skip,
                filtered,
                total,
                sw.Elapsed.TotalMilliseconds
            );
        }
        finally
        {
            FromToText.Value = $"{Skip.Value + 1} - {Skip.Value + _itemsSource.Count} ";
            progress.Report(1);
        }
    }

    private LogViewerViewModelConfig SaveLayout()
    {
        return new LogViewerViewModelConfig
        {
            SearchText = Search.Text.ViewValue.Value ?? string.Empty,
            Skip = Skip.Value,
            Take = Take.Value,
        };
    }

    private void LoadLayout(LogViewerViewModelConfig config)
    {
        Search.Text.ModelValue.Value = config.SearchText;
        Skip.Value = config.Skip;
        Take.Value = config.Take;
    }
}

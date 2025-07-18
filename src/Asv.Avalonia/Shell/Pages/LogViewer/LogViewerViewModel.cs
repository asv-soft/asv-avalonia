using System.Composition;
using System.Diagnostics;
using System.Globalization;
using Asv.Common;
using Avalonia.Threading;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public interface ILogViewerViewModel : IPage { }

[ExportPage(PageId)]
public class LogViewerViewModel
    : PageViewModel<ILogViewerViewModel>,
        ILogViewerViewModel,
        ISupportPagination,
        ISupportCancel,
        ISupportRefresh
{
    private readonly ILogService _logService;
    private readonly ISearchService _search;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ObservableList<LogMessageViewModel> _itemsSource = new();
    public const string PageId = "log";
    public const MaterialIconKind PageIcon = MaterialIconKind.Journal;

    public LogViewerViewModel()
        : base(DesignTime.Id, NullCommandService.Instance, DesignTime.LoggerFactory)
    {
        DesignTime.ThrowIfNotDesignMode();
        Title = RS.LogViewerViewModel_Title;
        Icon = PageIcon;
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
                this,
                DesignTime.LoggerFactory
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
                this,
                DesignTime.LoggerFactory
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
                this,
                DesignTime.LoggerFactory
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
                this,
                DesignTime.LoggerFactory
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
                this,
                DesignTime.LoggerFactory
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
                this,
                DesignTime.LoggerFactory
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
                this,
                DesignTime.LoggerFactory
            )
        );
    }

    [ImportingConstructor]
    public LogViewerViewModel(
        ICommandService cmd,
        ILogService logService,
        ISearchService search,
        ILoggerFactory loggerFactory
    )
        : base(PageId, cmd, loggerFactory)
    {
        _logService = logService;
        _search = search;
        _loggerFactory = loggerFactory;
        Title = RS.LogViewerViewModel_Title;
        Icon = PageIcon;
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

        Skip.Skip(1).Subscribe(_ => Search.Refresh());

        Next = new ReactiveCommand(_ => Commands.NextPage(this)).DisposeItWith(Disposable);
        Previous = new ReactiveCommand(_ => Commands.PreviousPage(this)).DisposeItWith(Disposable);

        Search.Refresh();
    }

    public SearchBoxViewModel Search { get; }

    public async Task UpdateImpl(
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
            Logger.ZLogTrace($"Start filtering log messages with filter: '{text}'");
            progress.Report(double.NaN);

            await foreach (
                var logMessage in _logService.LoadItemsFromLogFile(cancel).ConfigureAwait(false)
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
                        this,
                        _search,
                        text,
                        _loggerFactory,
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

    public BindableReactiveProperty<int> Skip { get; }
    public BindableReactiveProperty<int> Take { get; }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return Search;
        foreach (var item in Items)
        {
            yield return item;
        }
    }

    public void Refresh()
    {
        Search.Refresh();
    }

    public void Cancel()
    {
        Search.Cancel();
    }

    public override ValueTask<IRoutable> Navigate(NavigationId id)
    {
        if (id == Search.Id)
        {
            Search.Navigate(NavigationId.Empty);
            return new ValueTask<IRoutable>(Search);
        }

        var item = Items.FirstOrDefault(x => x.Id == id);
        if (item == null)
        {
            return ValueTask.FromResult<IRoutable>(this);
        }

        SelectedItem = item;
        return ValueTask.FromResult<IRoutable>(item);
    }

    protected override void AfterLoadExtensions() { }

    public override IExportInfo Source => SystemModule.Instance;
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
}

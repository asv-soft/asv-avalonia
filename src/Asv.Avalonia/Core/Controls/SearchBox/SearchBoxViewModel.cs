using Asv.Common;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

/// <summary>
/// Represents an asynchronous callback that applies a search query.
/// </summary>
/// <param name="text">The query text supplied by the search box.</param>
/// <param name="progress">Reports determinate progress from <c>0</c> to <c>1</c>.</param>
/// <param name="cancel">A token that is canceled when the current search should stop.</param>
/// <returns>A task that completes when the search operation finishes.</returns>
public delegate Task SearchDelegate(
    string? text,
    IProgress<double> progress,
    CancellationToken cancel
);

/// <summary>
/// Provides search input state and invokes an application-provided <see cref="SearchDelegate"/>
/// when the query changes.
/// </summary>
public class SearchBoxViewModel
    : ViewModel,
        ISupportTextSearch,
        ISupportRefresh,
        ISupportCancel,
        ISupportClear,
        IProgress<double>
{
    private readonly SearchDelegate _searchCallback;

    private readonly SynchronizedReactiveProperty<bool> _isExecuting;
    private readonly SynchronizedReactiveProperty<bool> _canExecute;
    private readonly SynchronizedReactiveProperty<double> _progress;

    private CancellationTokenSource? _cancellationTokenSource;
    private readonly ILogger<SearchBoxViewModel> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchBoxViewModel"/> class for design-time use.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the constructor is called outside Avalonia design mode.
    /// </exception>
    public SearchBoxViewModel()
        : this(DesignTime.Id.TypeId, DesignTime.LoggerFactory, (_, _, _) => Task.CompletedTask)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SearchBoxViewModel"/> class.
    /// </summary>
    /// <param name="typeId">The type identifier used to build the navigation id.</param>
    /// <param name="loggerFactory">The logger factory used to report callback failures.</param>
    /// <param name="searchCallback">The callback that applies each search query.</param>
    /// <param name="throttleTime">
    /// The optional debounce interval for text changes. When <c>null</c>, changes are processed immediately.
    /// </param>
    public SearchBoxViewModel(
        string typeId,
        ILoggerFactory loggerFactory,
        SearchDelegate searchCallback,
        TimeSpan? throttleTime = null
    )
        : base(typeId)
    {
        _searchCallback = searchCallback;
        _logger = loggerFactory.CreateLogger<SearchBoxViewModel>();
        var text = new ReactiveProperty<string?>(string.Empty).DisposeItWith(Disposable);
        Text = new HistoricalStringProperty("text", text, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        _isExecuting = new SynchronizedReactiveProperty<bool>().DisposeItWith(Disposable);
        _canExecute = new SynchronizedReactiveProperty<bool>(true).DisposeItWith(Disposable);
        _progress = new SynchronizedReactiveProperty<double>().DisposeItWith(Disposable);

        CanExecute = _canExecute.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
        IsExecuting = _isExecuting.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
        Progress = _progress.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);

        RefreshCommand = new ReactiveCommand((_, ct) => Refresh(ct)).DisposeItWith(Disposable);

        var textValueObservable = Text.ViewValue.Skip(1);

        if (throttleTime is not null)
        {
            textValueObservable = textValueObservable.Debounce(throttleTime.Value);
        }

        textValueObservable
            .DistinctUntilChanged()
            .WhereNotNull()
            .SubscribeAwait((x, cancel) => QueryAsync(x, cancel), AwaitOperation.Parallel)
            .DisposeItWith(Disposable);

        Disposable.AddAction(() => _cancellationTokenSource?.Cancel(false));
    }

    /// <summary>
    /// Gets the historical property that stores the current query text.
    /// </summary>
    public HistoricalStringProperty Text { get; }

    /// <summary>
    /// Gets the idle-state flag, which is <c>false</c> while the callback is running.
    /// </summary>
    public IReadOnlyBindableReactiveProperty<bool> CanExecute { get; }

    /// <summary>
    /// Gets a value indicating whether the search callback is currently running.
    /// </summary>
    public IReadOnlyBindableReactiveProperty<bool> IsExecuting { get; }

    /// <summary>
    /// Gets the current progress value. <see cref="double.NaN"/> represents indeterminate progress.
    /// </summary>
    public IReadOnlyBindableReactiveProperty<double> Progress { get; }

    /// <summary>
    /// Gets the command that reruns the current query.
    /// </summary>
    public ReactiveCommand RefreshCommand { get; }

    /// <summary>
    /// Cancels the current search and restores the idle state.
    /// </summary>
    public void Cancel()
    {
        _cancellationTokenSource?.Cancel(false);
        _cancellationTokenSource = null;
        _isExecuting.Value = false;
        _canExecute.Value = true;
        _progress.Value = 1;
        _logger.LogWarning("Search '{NavId}' was cancelled", Id);
    }

    /// <summary>
    /// Starts the current query again without applying the debounce interval.
    /// </summary>
    /// <param name="cancel">A token linked to the started search operation.</param>
    /// <returns>A completed task after the search operation has been started.</returns>
    public ValueTask Refresh(CancellationToken cancel = default)
    {
        Query(Text.ViewValue.Value, cancel);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Sets the query text to an empty string.
    /// </summary>
    public void Clear()
    {
        Text.ViewValue.Value = string.Empty;
    }

    /// <summary>
    /// Clears the query unless the supplied token is already canceled.
    /// </summary>
    /// <param name="cancel">A token that prevents the clear operation when already canceled.</param>
    /// <returns>A completed task.</returns>
    public ValueTask ClearCommandCall(CancellationToken cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return ValueTask.CompletedTask;
        }

        Clear();
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Returns the historical query property as the child view model.
    /// </summary>
    /// <returns>The child view models owned by the search box.</returns>
    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Text;
    }

    /// <summary>
    /// Focuses the search input and continues normal child navigation.
    /// </summary>
    /// <param name="id">The navigation id passed to the base implementation.</param>
    /// <param name="cancel">A token that cancels navigation.</param>
    /// <returns>The resolved view model.</returns>
    public override ValueTask<IViewModel> Navigate(NavId id, CancellationToken cancel = default)
    {
        if (cancel.IsCancellationRequested)
        {
            return ValueTask.FromResult<IViewModel>(this);
        }

        Focus();
        return base.Navigate(id, cancel);
    }

    /// <summary>
    /// Starts the specified query immediately.
    /// </summary>
    /// <param name="text">The query to start. <c>null</c> is treated as an empty string.</param>
    public void Query(string? text)
    {
        Query(text, CancellationToken.None);
    }

    /// <summary>
    /// Starts the specified query immediately with caller-controlled cancellation.
    /// </summary>
    /// <param name="text">The query to start. <c>null</c> is treated as an empty string.</param>
    /// <param name="cancel">A token linked to the started search operation.</param>
    public void Query(string? text, CancellationToken cancel)
    {
        if (_isExecuting.Value)
        {
            Cancel();
        }

        InternalExecuteAsync(text ?? string.Empty, cancel).SafeFireAndForget(ErrorHandler);
    }

    private ValueTask QueryAsync(string text, CancellationToken cancel)
    {
        Query(text, cancel);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Requests focus for the query text and its bound input.
    /// </summary>
    public void Focus()
    {
        Text.Focus();
    }

    private async Task InternalExecuteAsync(string text, CancellationToken cancel)
    {
        _isExecuting.Value = true;
        _canExecute.Value = false;
        _progress.Value = double.NaN;
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancel);

        try
        {
            await _searchCallback(text, this, _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            ErrorHandler(ex);
        }
        finally
        {
            _isExecuting.Value = false;
            _canExecute.Value = true;
            _progress.Value = 1;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private void ErrorHandler(Exception err)
    {
        _logger.LogError(err, "Error in search '{NavId}': {ErrMessage}", Id, err.Message);
        _isExecuting.Value = false;
        _canExecute.Value = true;
        _progress.Value = 1;
    }

    /// <summary>
    /// Updates the current search progress.
    /// </summary>
    /// <param name="value">The progress value exposed through <see cref="Progress"/>.</param>
    public void Report(double value) => _progress.Value = value;
}

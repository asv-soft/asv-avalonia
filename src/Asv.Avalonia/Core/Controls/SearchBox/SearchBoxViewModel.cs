using Asv.Common;
using Asv.Modeling;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public delegate Task SearchDelegate(
    string? text,
    IProgress<double> progress,
    CancellationToken cancel
);

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

    public SearchBoxViewModel()
        : this(DesignTime.Id.TypeId, DesignTime.LoggerFactory, (_, _, _) => Task.CompletedTask)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

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
            .SubscribeAwait((x, _) => QueryAsync(x), AwaitOperation.Parallel)
            .DisposeItWith(Disposable);

        Disposable.AddAction(() => _cancellationTokenSource?.Cancel(false));
    }

    public HistoricalStringProperty Text { get; }
    public IReadOnlyBindableReactiveProperty<bool> CanExecute { get; }
    public IReadOnlyBindableReactiveProperty<bool> IsExecuting { get; }
    public IReadOnlyBindableReactiveProperty<double> Progress { get; }
    public ReactiveCommand RefreshCommand { get; }

    public void Cancel()
    {
        _cancellationTokenSource?.Cancel(false);
        _cancellationTokenSource = null;
        _isExecuting.Value = false;
        _canExecute.Value = true;
        _progress.Value = 1;
        _logger.LogWarning("Search '{NavId}' was cancelled", Id);
    }

    public ValueTask Refresh(CancellationToken cancel = default)
    {
        Query(Text.ViewValue.Value);
        return ValueTask.CompletedTask;
    }

    public void Clear()
    {
        Text.ViewValue.Value = string.Empty;
    }

    public ValueTask ClearCommandCall()
    {
        Clear();
        return ValueTask.CompletedTask;
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Text;
    }

    public override ValueTask<IViewModel> Navigate(NavId id)
    {
        Focus();
        return base.Navigate(id);
    }

    public void Query(string? text)
    {
        if (_isExecuting.Value)
        {
            Cancel();
        }

        InternalExecuteAsync(text ?? string.Empty).SafeFireAndForget(ErrorHandler);
    }

    private ValueTask QueryAsync(string text)
    {
        Query(text);
        return ValueTask.CompletedTask;
    }

    public void Focus()
    {
        Text.Focus();
    }

    private async Task InternalExecuteAsync(string text)
    {
        _isExecuting.Value = true;
        _canExecute.Value = false;
        _progress.Value = double.NaN;
        _cancellationTokenSource = new CancellationTokenSource();

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

    public void Report(double value) => _progress.Value = value;
}

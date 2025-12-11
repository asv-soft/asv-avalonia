using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public delegate Task SearchDelegate(
    string? text,
    IProgress<double> progress,
    CancellationToken cancel
);

public class SearchBoxViewModel
    : RoutableViewModel,
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

    public SearchBoxViewModel()
        : this(DesignTime.Id, DesignTime.LoggerFactory, (_, _, _) => Task.CompletedTask)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public SearchBoxViewModel(
        NavigationId id,
        ILoggerFactory loggerFactory,
        SearchDelegate searchCallback,
        TimeSpan? throttleTime = null
    )
        : base(id, loggerFactory)
    {
        _searchCallback = searchCallback;

        var text = new ReactiveProperty<string?>(string.Empty).DisposeItWith(Disposable);
        Text = new HistoricalStringProperty(nameof(Text), text, loggerFactory)
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);

        _isExecuting = new SynchronizedReactiveProperty<bool>().DisposeItWith(Disposable);
        _canExecute = new SynchronizedReactiveProperty<bool>(true).DisposeItWith(Disposable);
        _progress = new SynchronizedReactiveProperty<double>().DisposeItWith(Disposable);

        CanExecute = _canExecute.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
        IsExecuting = _isExecuting.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
        Progress = _progress.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);

        var textValueObservable = Text.ViewValue.Skip(1);

        if (throttleTime is not null)
        {
            textValueObservable = textValueObservable.Debounce(throttleTime.Value);
        }

        textValueObservable
            .DistinctUntilChanged()
            .WhereNotNull()
            .SubscribeAwait(
                async (x, c) =>
                    await this.ExecuteCommand(
                        TextSearchCommand.Id,
                        CommandArg.CreateString(x),
                        cancel: c
                    ),
                AwaitOperation.Parallel
            )
            .DisposeItWith(Disposable);

        Disposable.AddAction(() => _cancellationTokenSource?.Cancel(false));
    }

    public HistoricalStringProperty Text { get; }
    public IReadOnlyBindableReactiveProperty<bool> CanExecute { get; }
    public IReadOnlyBindableReactiveProperty<bool> IsExecuting { get; }
    public IReadOnlyBindableReactiveProperty<double> Progress { get; }

    public void Cancel()
    {
        _cancellationTokenSource?.Cancel(false);
        _cancellationTokenSource = null;
        _isExecuting.Value = false;
        _canExecute.Value = true;
        _progress.Value = 1;
        Logger.LogWarning("Search '{NavigationId}' was cancelled", Id);
    }

    public void Refresh()
    {
        Query(Text.ViewValue.Value);
    }

    public void Clear()
    {
        Text.ViewValue.Value = string.Empty;
    }

    public async ValueTask ClearCommandCall()
    {
        await this.ExecuteCommand(ClearCommand.Id);
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return Text;
    }

    public override ValueTask<IRoutable> Navigate(NavigationId id)
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
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception ex)
        {
            ErrorHandler(ex);
            return;
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
        Logger.LogError(err, "Error in search '{NavigationId}': {ErrMessage}", Id, err.Message);
        _isExecuting.Value = false;
        _canExecute.Value = true;
        _progress.Value = 1;
    }

    public void Report(double value) => _progress.Value = value;
}

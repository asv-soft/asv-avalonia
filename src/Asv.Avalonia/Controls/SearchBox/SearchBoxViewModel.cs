using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public delegate Task SearchDelegate(
    string? text,
    IProgress<double> progress,
    CancellationToken cancel
);

public class SearchBoxViewModel : RoutableViewModel, ISupportTextSearch, IProgress<double>
{
    private readonly SearchDelegate _searchCallback;

    private readonly BindableReactiveProperty<bool> _isExecuting;
    private readonly BindableReactiveProperty<bool> _canExecute;
    private readonly BindableReactiveProperty<double> _progress;

    private string _searchText = string.Empty;
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

        Text = new BindableReactiveProperty<string>(string.Empty).DisposeItWith(Disposable);

        _isExecuting = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        _canExecute = new BindableReactiveProperty<bool>(true).DisposeItWith(Disposable);
        _progress = new BindableReactiveProperty<double>().DisposeItWith(Disposable);

        if (throttleTime != null)
        {
            Text.Skip(1)
                .Debounce(throttleTime.Value)
                .SubscribeAwait(
                    async (x, _) => await TextSearchCommand.Execute(this, x),
                    AwaitOperation.Parallel
                )
                .DisposeItWith(Disposable);
        }

        Disposable.AddAction(() => _cancellationTokenSource?.Cancel(false));
    }

    public BindableReactiveProperty<string> Text { get; }

    public bool IsSelected
    {
        get;
        set => SetField(ref field, value);
    }

    public string SearchText => _searchText;

    public void QueryWithCommand(string? text)
    {
        TextSearchCommand.Execute(this, text);
    }

    public void Query(string? text)
    {
        Logger.ZLogDebug($"Begin search '{Id}' with text '{text}'");
        if (_isExecuting.Value)
        {
            Cancel();
        }

        InternalExecuteAsync(text ?? string.Empty).SafeFireAndForget(ErrorHandler);
    }

    private void ErrorHandler(Exception err)
    {
        Logger.LogError(err, $"Error in search '{Id}': {err.Message}");
        _isExecuting.Value = false;
        _canExecute.Value = true;
        _progress.Value = 1;
    }

    private async Task InternalExecuteAsync(string text)
    {
        Text.Value = text;
        _searchText = text;
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
        }
    }

    public BindableReactiveProperty<bool> CanExecute => _canExecute;
    public BindableReactiveProperty<bool> IsExecuting => _isExecuting;
    public BindableReactiveProperty<double> Progress => _progress;

    public void Clear()
    {
        Text.Value = string.Empty;
    }

    public void Cancel()
    {
        _cancellationTokenSource?.Cancel(false);
        _cancellationTokenSource = null;
        _isExecuting.Value = false;
        _canExecute.Value = true;
        _progress.Value = 1;
        Logger.LogWarning($"Search '{Id}' was cancelled");
    }

    public void Refresh()
    {
        Query(Text.Value);
    }

    public void Focus()
    {
        IsSelected = false;
        IsSelected = true;
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        return [];
    }

    public override ValueTask<IRoutable> Navigate(NavigationId id)
    {
        Focus();
        return base.Navigate(id);
    }

    public void Report(double value) => _progress.Value = value;
}

﻿using Asv.Common;
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

    private readonly BindableReactiveProperty<bool> _isExecuting;
    private readonly BindableReactiveProperty<bool> _canExecute;
    private readonly BindableReactiveProperty<double> _progress;

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
        Text = new HistoricalStringProperty(nameof(Text), text, loggerFactory, this).DisposeItWith(
            Disposable
        );

        _isExecuting = new BindableReactiveProperty<bool>().DisposeItWith(Disposable);
        _canExecute = new BindableReactiveProperty<bool>(true).DisposeItWith(Disposable);
        _progress = new BindableReactiveProperty<double>().DisposeItWith(Disposable);

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

    public bool IsSelected
    {
        get;
        set => SetField(ref field, value);
    }

    public void Query(string? text)
    {
        if (_isExecuting.Value)
        {
            Cancel();
        }

        InternalExecuteAsync(text ?? string.Empty).SafeFireAndForget(ErrorHandler);
    }

    private void ErrorHandler(Exception err)
    {
        Logger.LogError(err, "Error in search '{NavigationId}': {ErrMessage}", Id, err.Message);
        _isExecuting.Value = false;
        _canExecute.Value = true;
        _progress.Value = 1;
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
            _cancellationTokenSource.Dispose();
            _cancellationTokenSource = null;
        }
    }

    public IReadOnlyBindableReactiveProperty<bool> CanExecute =>
        _canExecute.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
    public IReadOnlyBindableReactiveProperty<bool> IsExecuting =>
        _isExecuting.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);
    public IReadOnlyBindableReactiveProperty<double> Progress =>
        _progress.ToReadOnlyBindableReactiveProperty().DisposeItWith(Disposable);

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

    public void Focus()
    {
        IsSelected = false;
        IsSelected = true;
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

    public void Report(double value) => _progress.Value = value;
}

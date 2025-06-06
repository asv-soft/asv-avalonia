﻿using System.Diagnostics;
using Asv.Common;
using Avalonia.Metadata;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public delegate void ProgressDelegate(string text, double progress);

public delegate Task BackgroundCommandDelegate<in TArg>(
    TArg arg,
    ProgressDelegate callback,
    CancellationToken cancel
);

public class BackgroundCommand<TArg> : ViewModelBase
{
    private readonly BackgroundCommandDelegate<TArg> _execute;
    private readonly BindableReactiveProperty<bool> _isExecuting = new();
    private readonly BindableReactiveProperty<bool> _canExecute = new();
    private readonly BindableReactiveProperty<double> _progress = new();
    private readonly BindableReactiveProperty<string> _progressMessage = new();
    private readonly ILogger<BackgroundCommand<TArg>> _logger;
    private CancellationTokenSource? _cancellationTokenSource;

    public BackgroundCommand(
        string id,
        BackgroundCommandDelegate<TArg> execute,
        ILoggerFactory loggerFactory
    )
        : base(id)
    {
        _execute = execute;
        _logger = loggerFactory.CreateLogger<BackgroundCommand<TArg>>();
    }

    public void Execute(TArg arg)
    {
        if (IsExecuting.Value)
        {
            Cancel();
        }

        _cancellationTokenSource = new CancellationTokenSource();
        InternalExecute(arg).SafeFireAndForget(ErrorHandler);
    }

    private void ErrorHandler(Exception err)
    {
        _logger.LogError(err, $"Command '{Id}' execution failed");
        _isExecuting.Value = false;
        _canExecute.Value = true;
        _progress.Value = 1;
        _progressMessage.Value = "Execution failed";
    }

    private async Task InternalExecute(TArg arg)
    {
        _isExecuting.Value = true;
        _canExecute.Value = false;
        _progress.Value = 0;
        _progressMessage.Value = "Executing...";

        try
        {
            Debug.Assert(
                _cancellationTokenSource != null,
                nameof(_cancellationTokenSource) + " != null"
            );
            await _execute(
                arg,
                (text, progress) =>
                {
                    _progressMessage.Value = text;
                    _progress.Value = progress;
                },
                _cancellationTokenSource.Token
            );
        }
        catch (OperationCanceledException)
        {
            Cancel();
            return;
        }
        catch (Exception ex)
        {
            ErrorHandler(ex);
            return;
        }

        _isExecuting.Value = false;
        _canExecute.Value = true;
        _progress.Value = 1;
        _progressMessage.Value = "Execution completed";
    }

    public void Cancel()
    {
        _cancellationTokenSource?.Cancel(false);
        _isExecuting.Value = false;
        _canExecute.Value = true;
        _progress.Value = 1;
        _logger.LogWarning($"Command '{Id}' was cancelled");
    }

    public IReadOnlyBindableReactiveProperty<bool> IsExecuting => _isExecuting;

    public IReadOnlyBindableReactiveProperty<bool> CanExecute => _canExecute;

    public IReadOnlyBindableReactiveProperty<double> Progress => _progress;

    public IReadOnlyBindableReactiveProperty<string> ProgressMessage => _progressMessage;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _isExecuting.Dispose();
            _canExecute.Dispose();
            _progress.Dispose();
            _progressMessage.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}

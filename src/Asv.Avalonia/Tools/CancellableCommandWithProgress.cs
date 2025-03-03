using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia;

public static class CoreDesignTime
{
    public static CancellableCommandWithProgress CancellableCommand() =>
        new((_, _) => Task.CompletedTask, "Default", NullLoggerFactory.Instance);
}

public delegate Task CommandExecuteDelegate(IProgress<double> progress, CancellationToken cancel);

public class CancellableCommandWithProgress : IProgress<double>, IDisposable
{
    private readonly CommandExecuteDelegate _execute;
    private readonly ReactiveCommand _cancelCommand;
    private readonly ReactiveCommand _command;
    private readonly ILogger<CancellableCommandWithProgress> _logger;

    public CancellableCommandWithProgress(
        CommandExecuteDelegate execute,
        string name,
        ILoggerFactory logFactory
    )
    {
        _logger = logFactory.CreateLogger<CancellableCommandWithProgress>();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
        }

        _execute = execute ?? throw new ArgumentNullException(nameof(execute));

        Title = new BindableReactiveProperty<string>(name);
        IsExecuting = new BindableReactiveProperty<bool>();
        CanExecute = new BindableReactiveProperty<bool>(true);
        Progress = new BindableReactiveProperty<double>();

        _command = new ReactiveCommand(_ => Task.Run(() => InternalExecute(default)));
        _cancelCommand = new ReactiveCommand(_ => Task.Run(InternalCancel));
    }

    public BindableReactiveProperty<string> Title { get; set; }
    public BindableReactiveProperty<bool> IsExecuting { get; set; }
    public BindableReactiveProperty<bool> CanExecute { get; set; }
    public BindableReactiveProperty<double> Progress { get; set; }

    public ICommand Cancel => _cancelCommand;
    public ICommand Command => _command;

    private void InternalError(Exception exception)
    {
        _logger.LogError(exception, $"Error to execute command {Title}:{exception.Message}");
    }

    private void InternalCancel()
    {
        _logger.LogWarning($"Command '{Title}' was cancelled");
    }

    private async ValueTask InternalExecute(CancellationToken ct)
    {
        if (!CanExecute.Value)
        {
            return;
        }

        IsExecuting.OnNext(true);
        CanExecute.OnNext(false);
        Progress.OnNext(0);

        try
        {
            _logger.LogInformation($"Start command '{Title}'");
            await _execute(this, ct);
            _logger.LogInformation($"Command '{Title}' is completed");
        }
        catch (Exception e)
        {
            InternalError(e);
        }
        finally
        {
            Progress.OnNext(0);
            CanExecute.OnNext(true);
            IsExecuting.OnNext(false);
        }
    }

    public void Report(double value)
    {
        Progress.OnNext(value);
    }

    public async ValueTask ExecuteAsync(CancellationToken cancel)
    {
        await using var sub = cancel.Register(() => _cancelCommand.WaitAsync(cancel));
        await _command.WaitAsync(cancel);
    }

    #region Dispose

    private volatile int _isDisposed;
    public bool IsDisposed => _isDisposed != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ThrowIfDisposed()
    {
        if (_isDisposed == 0)
        {
            return;
        }

        throw new ObjectDisposedException(GetType().FullName);
    }

    public void Dispose()
    {
        // Make sure we're the first call to Dispose
        if (Interlocked.CompareExchange(ref _isDisposed, 1, 0) == 1)
        {
            return;
        }

        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        _cancelCommand.Dispose();
        _command.Dispose();
        IsExecuting.Dispose();
        Title.Dispose();
    }

    #endregion
}

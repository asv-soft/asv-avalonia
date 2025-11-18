using Asv.Common;
using Avalonia.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public static class CoreDesignTime
{
    public static CancellableCommandWithProgress<TArg> CancellableCommand<TArg>() =>
        new((_, _, _) => Task.CompletedTask, "Default", NullLoggerFactory.Instance);
}

public delegate Task CommandExecuteDelegate<in TArg>(
    TArg arg,
    IProgress<double> progress,
    CancellationToken cancel
);

public class CancellableCommandWithProgress<TArg> : AsyncDisposableOnce, IProgress<double>
{
    private readonly CommandExecuteDelegate<TArg> _execute;
    private readonly ReactiveCommand _cancelCommand;
    private readonly ReactiveCommand<TArg> _command;
    private readonly ILogger<CancellableCommandWithProgress<TArg>> _logger;
    private readonly Lock _sync = new();
    private CancellationTokenSource? _cancellationTokenSource;

    public CancellableCommandWithProgress(
        CommandExecuteDelegate<TArg> execute,
        string title,
        ILoggerFactory logFactory
    )
    {
        _logger = logFactory.CreateLogger<CancellableCommandWithProgress<TArg>>();

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(title));
        }

        _execute = execute ?? throw new ArgumentNullException(nameof(execute));

        Title = new BindableReactiveProperty<string>(title);
        IsExecuting = new BindableReactiveProperty<bool>();
        CanExecute = new BindableReactiveProperty<bool>(true);
        Progress = new BindableReactiveProperty<double>(0);
        _command = new ReactiveCommand<TArg>(InternalExecute);
        _cancelCommand = new ReactiveCommand(InternalCancel);
    }

    private void InternalCancel(Unit obj)
    {
        using (_sync.EnterScope())
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            _cancelCommand.ChangeCanExecute(false);
            _command.ChangeCanExecute(true);
            IsExecuting.Value = false;
            CanExecute.Value = true;
            Progress.Value = 1;
        });

        _logger.LogWarning("Command '{Title}' was cancelled", Title);
    }

    private async ValueTask InternalExecute(TArg arg, CancellationToken token = default)
    {
        try
        {
            if (IsExecuting.Value)
            {
                InternalCancel(Unit.Default);
            }

            if (!Dispatcher.UIThread.CheckAccess()) { }

            using (_sync.EnterScope())
            {
                _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            }

            Dispatcher.UIThread.Invoke(() =>
            {
                _cancelCommand.ChangeCanExecute(true);
                _command.ChangeCanExecute(false);
                IsExecuting.Value = true;
                CanExecute.Value = false;
                Progress.Value = 0;
            });

            _logger.ZLogTrace($"Start command '{Title}' ");
            await _execute(arg, this, _cancellationTokenSource.Token).ConfigureAwait(false);
            _logger.ZLogTrace($"Command '{Title}' completed successfully");
            Dispatcher.UIThread.Invoke(() =>
            {
                _cancelCommand.ChangeCanExecute(false);
                _command.ChangeCanExecute(true);
                IsExecuting.Value = false;
                CanExecute.Value = true;
                Progress.Value = 1;
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Command '{Title}' was cancelled by user", Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error to execute command {Title}", Title);
        }
    }

    public BindableReactiveProperty<string> Title { get; }
    public BindableReactiveProperty<bool> IsExecuting { get; }
    public BindableReactiveProperty<bool> CanExecute { get; }
    public BindableReactiveProperty<double> Progress { get; }

    public ReactiveCommand Cancel => _cancelCommand;
    public ReactiveCommand<TArg> Command => _command;

    public void Report(double value)
    {
        Dispatcher.UIThread.Invoke(() => Progress.Value = value);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cancelCommand.Dispose();
            _command.Dispose();
            _cancellationTokenSource?.Dispose();
            Title.Dispose();
            IsExecuting.Dispose();
            CanExecute.Dispose();
            Progress.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        await CastAndDispose(_cancelCommand);
        await CastAndDispose(_command);
        if (_cancellationTokenSource != null)
        {
            await CastAndDispose(_cancellationTokenSource);
        }

        await CastAndDispose(Title);
        await CastAndDispose(IsExecuting);
        await CastAndDispose(CanExecute);
        await CastAndDispose(Progress);

        await base.DisposeAsyncCore();

        return;

        static async ValueTask CastAndDispose(IDisposable resource)
        {
            if (resource is IAsyncDisposable resourceAsyncDisposable)
            {
                await resourceAsyncDisposable.DisposeAsync();
            }
            else
            {
                resource.Dispose();
            }
        }
    }

    public void Execute(TArg arg) => Command.Execute(arg);
}

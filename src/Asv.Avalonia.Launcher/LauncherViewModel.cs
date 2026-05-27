using Asv.Avalonia.Launcher.Api;
using Asv.Avalonia.Launcher.Contracts;
using Asv.Avalonia.Launcher.Orchestration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using R3;

namespace Asv.Avalonia.Launcher;

public sealed class LauncherViewModel : IDisposable
{
    private readonly CancellationTokenSource _lifecycleCts;
    private readonly string[] _startupArgs;
    private readonly ILauncherOrchestrator _orchestrator;
    private readonly LauncherApplicationOptions _applicationOptions;
    private readonly SynchronizedReactiveProperty<string> _status;
    private readonly SynchronizedReactiveProperty<double> _progress;
    private readonly SynchronizedReactiveProperty<bool> _isProgressIndeterminate;
    private readonly SynchronizedReactiveProperty<bool> _isCloseVisible;
    private DisposableBag _disposable;
    private bool _isDisposed;
    private int _isShutdownRequested;
    private volatile int _isStarted;

    public LauncherViewModel()
        : this(
            [],
            NullLauncherOrchestrator.Instance,
            new LauncherApplicationOptions { ShutdownOnSuccess = false }
        )
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException(
                "Design constructor should not be used at runtime."
            );
        }
    }

    public LauncherViewModel(
        IReadOnlyList<string> startupArgs,
        ILauncherOrchestrator orchestrator,
        LauncherApplicationOptions? applicationOptions = null
    )
    {
        ArgumentNullException.ThrowIfNull(startupArgs);
        ArgumentNullException.ThrowIfNull(orchestrator);
        ArgumentNullException.ThrowIfNull(applicationOptions);

        _disposable = default;
        _lifecycleCts = new CancellationTokenSource();
        _startupArgs = startupArgs.ToArray();
        _orchestrator = orchestrator;
        _applicationOptions = applicationOptions;

        _status = new SynchronizedReactiveProperty<string>(
            "Launcher is starting application..."
        ).AddTo(ref _disposable);
        _progress = new SynchronizedReactiveProperty<double>().AddTo(ref _disposable);
        _isProgressIndeterminate = new SynchronizedReactiveProperty<bool>(true).AddTo(
            ref _disposable
        );
        _isCloseVisible = new SynchronizedReactiveProperty<bool>().AddTo(ref _disposable);
        CloseCommand = new ReactiveCommand(_ => ShutdownApplication()).AddTo(ref _disposable);

        Status = _status
            .ObserveOnUIThreadDispatcher()
            .ToReadOnlyBindableReactiveProperty<string>()
            .AddTo(ref _disposable);
        Progress = _progress
            .ObserveOnUIThreadDispatcher()
            .ToReadOnlyBindableReactiveProperty()
            .AddTo(ref _disposable);
        IsProgressIndeterminate = _isProgressIndeterminate
            .ObserveOnUIThreadDispatcher()
            .ToReadOnlyBindableReactiveProperty()
            .AddTo(ref _disposable);
        IsCloseVisible = _isCloseVisible
            .ObserveOnUIThreadDispatcher()
            .ToReadOnlyBindableReactiveProperty()
            .AddTo(ref _disposable);

        if (!Design.IsDesignMode)
        {
            StartInBackground();
        }
    }

    public IReadOnlyBindableReactiveProperty<string> Status { get; }
    public IReadOnlyBindableReactiveProperty<double> Progress { get; }
    public IReadOnlyBindableReactiveProperty<bool> IsProgressIndeterminate { get; }
    public IReadOnlyBindableReactiveProperty<bool> IsCloseVisible { get; }
    public ReactiveCommand CloseCommand { get; }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _lifecycleCts.Cancel();
        _disposable.Dispose();
        _lifecycleCts.Dispose();
    }

    private void StartInBackground()
    {
        if (Interlocked.CompareExchange(ref _isStarted, 1, 0) != 0)
        {
            return;
        }

        Task.Run(async () => await StartAsync(_lifecycleCts.Token), _lifecycleCts.Token);
    }

    private async Task StartAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        try
        {
            await RunOrchestrationAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Window closed while orchestration was running.
        }
        catch (Exception ex)
        {
            Environment.ExitCode = (int)LauncherExitCode.UnexpectedError;
            SetFailed($"Unexpected launcher error: {ex.Message}");
        }
    }

    private async Task RunOrchestrationAsync(CancellationToken cancellationToken)
    {
        SetStatus("Parsing launcher arguments...", null);

        if (!LauncherCommandLineParser.TryParse(_startupArgs, out var options, out var parseError))
        {
            Environment.ExitCode = (int)LauncherExitCode.InvalidArguments;
            SetFailed(parseError);
            return;
        }

        ArgumentNullException.ThrowIfNull(options);

        SetStatus("Starting target process...", 0.05);

        var progress = new Progress<LauncherSignal>(signal =>
        {
            SetStatus(signal.Message ?? signal.Type.ToString(), signal.Progress);
        });

        var runResult = await _orchestrator
            .RunAsync(options, progress, cancellationToken)
            .ConfigureAwait(false);
        Environment.ExitCode = (int)runResult.ExitCode;

        if (runResult.ExitCode == LauncherExitCode.Success)
        {
            SetStatus(runResult.Message, 1);

            if (_applicationOptions.ShutdownOnSuccess)
            {
                await Task.Delay(_applicationOptions.SuccessShutdownDelay, cancellationToken)
                    .ConfigureAwait(false);
                ShutdownApplication();
            }

            return;
        }

        SetFailed(runResult.Message);
    }

    private void SetStatus(string status, double? progress = null)
    {
        if (_isDisposed)
        {
            return;
        }

        _status.Value = status;

        if (progress is >= 0 and <= 1)
        {
            _isProgressIndeterminate.Value = false;
            _progress.Value = progress.Value;
        }
        else
        {
            _isProgressIndeterminate.Value = true;
        }
    }

    private void SetFailed(string message)
    {
        if (_isDisposed)
        {
            return;
        }

        SetStatus(message, 0);
        _isCloseVisible.Value = true;
    }

    private void ShutdownApplication()
    {
        if (_isDisposed || Interlocked.Exchange(ref _isShutdownRequested, 1) != 0)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (
                Application.Current?.ApplicationLifetime
                is IClassicDesktopStyleApplicationLifetime lifetime
            )
            {
                lifetime.Shutdown();
            }
        });
    }
}

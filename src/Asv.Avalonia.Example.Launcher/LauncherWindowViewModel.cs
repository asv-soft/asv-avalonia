using System.ComponentModel;
using System.Runtime.CompilerServices;
using Asv.Avalonia.Example.Launcher.Contracts;
using Asv.Avalonia.Example.Launcher.Orchestration;
using Asv.Avalonia.Launcher.Api;
using Avalonia.Controls;
using R3;

namespace Asv.Avalonia.Example.Launcher;

public sealed class LauncherWindowViewModel
    : INotifyPropertyChanging,
        INotifyPropertyChanged,
        IDisposable
{
    #region Common

    private DisposableBag _disposable;
    private bool _isDisposed;

    public event PropertyChangingEventHandler? PropertyChanging;
    public event PropertyChangedEventHandler? PropertyChanged;

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        _disposable.Dispose();
    }

    private void OnPropertyChanging([CallerMemberName] string? propertyName = null)
    {
        PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        OnPropertyChanging(propertyName);
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion

    private readonly string[] _startupArgs;
    private readonly ILauncherOrchestrator _orchestrator;
    private readonly BindableReactiveProperty<string> _status;
    private readonly BindableReactiveProperty<double> _progress;
    private readonly BindableReactiveProperty<bool> _isProgressIndeterminate;
    private readonly BindableReactiveProperty<bool> _isCloseVisible;

    private string _statusValue = "Launcher is starting application...";
    private double _progressValue;
    private bool _isProgressIndeterminateValue = true;
    private bool _isCloseVisibleValue;

    private volatile int _isStarted;

    public LauncherWindowViewModel()
        : this([])
    {
        if (!Design.IsDesignMode)
        {
            throw new Exception("Should not be used in runtime.");
        }
    }

    public LauncherWindowViewModel(
        IReadOnlyList<string> startupArgs,
        ILauncherOrchestrator? orchestrator = null
    )
    {
        ArgumentNullException.ThrowIfNull(startupArgs);

        _disposable = default;
        _startupArgs = startupArgs.ToArray();
        _orchestrator = orchestrator ?? new LauncherOrchestrator();

        _status = new BindableReactiveProperty<string>(_statusValue).AddTo(ref _disposable);
        _progress = new BindableReactiveProperty<double>(_progressValue).AddTo(ref _disposable);
        _isProgressIndeterminate = new BindableReactiveProperty<bool>(
            _isProgressIndeterminateValue
        ).AddTo(ref _disposable);
        _isCloseVisible = new BindableReactiveProperty<bool>(_isCloseVisibleValue).AddTo(
            ref _disposable
        );

        _status
            .Subscribe(value => SetField(ref _statusValue, value, nameof(Status)))
            .AddTo(ref _disposable);
        _progress
            .Subscribe(value => SetField(ref _progressValue, value, nameof(Progress)))
            .AddTo(ref _disposable);
        _isProgressIndeterminate
            .Subscribe(value =>
                SetField(ref _isProgressIndeterminateValue, value, nameof(IsProgressIndeterminate))
            )
            .AddTo(ref _disposable);
        _isCloseVisible
            .Subscribe(value => SetField(ref _isCloseVisibleValue, value, nameof(IsCloseVisible)))
            .AddTo(ref _disposable);

        CloseCommand = new ReactiveCommand(_ => RequestClose()).AddTo(ref _disposable);
    }

    public string Status => _statusValue;
    public double Progress => _progressValue;
    public bool IsProgressIndeterminate => _isProgressIndeterminateValue;
    public bool IsCloseVisible => _isCloseVisibleValue;
    public ReactiveCommand CloseCommand { get; }

    public event EventHandler? CloseRequested;

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (Interlocked.CompareExchange(ref _isStarted, 1, 0) != 0)
        {
            return;
        }

        try
        {
            await RunOrchestrationAsync(cancellationToken);
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

        var runResult = await _orchestrator.RunAsync(options, progress, cancellationToken);
        Environment.ExitCode = (int)runResult.ExitCode;

        if (runResult.ExitCode == LauncherExitCode.Success)
        {
            RequestClose();
            return;
        }

        SetFailed(runResult.Message);
    }

    private void SetStatus(string status, double? progress = null)
    {
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
        SetStatus(message, 0);
        _isCloseVisible.Value = true;
    }

    private void RequestClose()
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }
}

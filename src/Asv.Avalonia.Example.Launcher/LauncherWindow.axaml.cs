using System;
using System.Threading;
using System.Threading.Tasks;
using Asv.Avalonia.Example.Launcher.Contracts;
using Asv.Avalonia.Example.Launcher.Orchestration;
using Asv.Avalonia.Launcher.Api;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Asv.Avalonia.Example.Launcher;

public partial class LauncherWindow : Window
{
    private readonly CancellationTokenSource _lifecycleCts = new();
    private bool _isOrchestrationStarted;

    public LauncherWindow()
    {
        InitializeComponent();
        SetStatus("Launcher is starting application...");
    }

    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        if (_isOrchestrationStarted)
        {
            return;
        }

        _isOrchestrationStarted = true;
        try
        {
            await RunOrchestrationAsync(_lifecycleCts.Token);
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

    protected override void OnClosed(EventArgs e)
    {
        _lifecycleCts.Cancel();
        _lifecycleCts.Dispose();
        base.OnClosed(e);
    }

    private async Task RunOrchestrationAsync(CancellationToken cancellationToken)
    {
        SetStatus("Parsing launcher arguments...", null);

        if (
            !LauncherCommandLineParser.TryParse(
                Program.StartupArgs,
                out var options,
                out var parseError
            )
        )
        {
            Environment.ExitCode = (int)LauncherExitCode.InvalidArguments;
            SetFailed(parseError);
            return;
        }

        ArgumentNullException.ThrowIfNull(options);

        SetStatus("Starting target process...", 0.05);

        var orchestrator = new LauncherOrchestrator();
        var progress = new Progress<LauncherSignal>(signal =>
        {
            SetStatus(signal.Message ?? signal.Type.ToString(), signal.Progress);
        });

        var runResult = await orchestrator.RunAsync(options, progress, cancellationToken);
        Environment.ExitCode = (int)runResult.ExitCode;

        if (runResult.ExitCode == LauncherExitCode.Success)
        {
            Close();
            return;
        }

        SetFailed(runResult.Message);
    }

    private void SetStatus(string status, double? progress = null)
    {
        StatusText.Text = status;

        if (progress is >= 0 and <= 1)
        {
            LaunchProgress.IsIndeterminate = false;
            LaunchProgress.Value = progress.Value;
        }
        else
        {
            LaunchProgress.IsIndeterminate = true;
        }
    }

    private void SetFailed(string message)
    {
        SetStatus(message, 0);
        CloseButton.IsVisible = true;
    }

    private void CloseButtonOnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}

using Asv.Avalonia.Example.Launcher.Orchestration;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example.Launcher;

public partial class App : Application
{
    private LauncherWindowViewModel? _viewModel;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var orchestrator = new LauncherOrchestrator();

            _viewModel = new LauncherWindowViewModel(Program.StartupArgs, orchestrator);
            desktop.MainWindow = new LauncherWindow { DataContext = _viewModel };
            desktop.Exit += (_, _) => DisposeViewModel();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisposeViewModel()
    {
        _viewModel?.Dispose();
        _viewModel = null;
    }
}

using Asv.Avalonia.Launcher.Orchestration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Themes.Fluent;

namespace Asv.Avalonia.Launcher;

public class LauncherApp : Application
{
    private LauncherViewModel? _viewModel;

    public override void Initialize()
    {
        if (Styles.Count == 0)
        {
            Styles.Add(new FluentTheme());
        }
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var options = CreateOptions();
            _viewModel = CreateViewModel(options);
            desktop.MainWindow = CreateMainWindow(_viewModel, options);
            desktop.Exit += (_, _) => DisposeViewModel();
        }

        base.OnFrameworkInitializationCompleted();
    }

    protected virtual void ConfigureLauncher(LauncherApplicationOptions options) { }

    protected virtual LauncherApplicationOptions CreateOptions()
    {
        var options = new LauncherApplicationOptions
        {
            Args = Environment.GetCommandLineArgs().Skip(1).ToArray(),
        };

        ConfigureLauncher(options);
        return options;
    }

    protected virtual LauncherViewModel CreateViewModel(LauncherApplicationOptions options)
    {
        var args = options.Args ?? Environment.GetCommandLineArgs().Skip(1).ToArray();
        var orchestrator = options.Orchestrator ?? new LauncherOrchestrator();
        return new LauncherViewModel(args, orchestrator, options);
    }

    protected virtual Window CreateMainWindow(
        LauncherViewModel viewModel,
        LauncherApplicationOptions options
    )
    {
        var window = options.CreateWindow?.Invoke(viewModel);
        if (window is not null)
        {
            if (window.DataContext is null)
            {
                window.DataContext = viewModel;
            }

            options.ConfigureWindow?.Invoke(window);
            return window;
        }

        var view = options.CreateView?.Invoke(viewModel) ?? new DefaultLauncherView();
        if (view.DataContext is null)
        {
            view.DataContext = viewModel;
        }

        window = new LauncherWindow { Content = view, DataContext = viewModel };
        options.ConfigureWindow?.Invoke(window);
        return window;
    }

    private void DisposeViewModel()
    {
        _viewModel?.Dispose();
        _viewModel = null;
    }
}

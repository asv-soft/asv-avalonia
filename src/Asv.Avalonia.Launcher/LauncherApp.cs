using Asv.Avalonia.Launcher.Orchestration;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;

namespace Asv.Avalonia.Launcher;

public class LauncherApp : Application
{
    private LauncherViewModel? _viewModel;

    public override void Initialize()
    {
        RequestedThemeVariant = ThemeVariant.Default;

        if (Styles.Count == 0)
        {
            Styles.Add(new FluentTheme());
        }

        base.Initialize();
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

    private LauncherApplicationOptions CreateOptions()
    {
        var options = new LauncherApplicationOptions
        {
            Args = Environment.GetCommandLineArgs().Skip(1).ToArray(),
        };

        ConfigureLauncher(options);
        return options;
    }

    private Window CreateMainWindow(LauncherViewModel viewModel, LauncherApplicationOptions options)
    {
        var view = options.CreateView?.Invoke(viewModel) ?? new DefaultLauncherView(options);
        if (view.DataContext is null)
        {
            view.DataContext = viewModel;
        }

        var window = new LauncherWindow { Content = view, DataContext = viewModel };
        options.ConfigureWindow?.Invoke(window);
        return window;
    }

    private LauncherViewModel CreateViewModel(LauncherApplicationOptions options)
    {
        var args = options.Args ?? Environment.GetCommandLineArgs().Skip(1).ToArray();
        var orchestrator = new LauncherOrchestrator();
        return new LauncherViewModel(args, orchestrator, options);
    }

    private void DisposeViewModel()
    {
        _viewModel?.Dispose();
        _viewModel = null;
    }
}

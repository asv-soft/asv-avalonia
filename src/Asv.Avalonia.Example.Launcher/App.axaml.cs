using Asv.Avalonia.Launcher;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example.Launcher;

public partial class App : LauncherApp
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    protected override void ConfigureLauncher(LauncherApplicationOptions options)
    {
        // Configure base launcher view
        options.IconSource = new Uri(
            "avares://Asv.Avalonia.Example.Launcher/Assets/asv-soft-logo.png"
        );
        options.Title = "Launcher for Asv.Avalonia Example";
        options.Description = "Example application startup launcher";
        options.Footer = "made by ASV.SOFT team";

        // Configure launcher view and window
        // options.CreateView = viewModel => new ExampleLauncherView { DataContext = viewModel };
        //
        // options.ConfigureWindow = window =>
        // {
        //     window.Title = "ASV Example Launcher";
        //     window.Width = 600;
        //     window.Height = 280;
        //     window.CanResize = false;
        //     window.Topmost = true;
        //     window.WindowDecorations = WindowDecorations.None;
        //     window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        // };

        // Configure Window Example
        // options.ConfigureWindow = window =>
        // {
        //     window.Title = "ASV Example Launcher";
        //     window.Width = 1000;
        //     window.Height = 320;
        //     window.MinWidth = 420;
        //     window.MinHeight = 260;
        //     window.CanResize = true;
        //     window.Topmost = false;
        //     window.WindowDecorations = WindowDecorations.Full;
        //     window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        // };

        // Configure launcher behavior
        // options.SuccessShutdownDelay = TimeSpan.FromSeconds(2);
        // options.ShutdownOnSuccess = false;
    }
}

using Asv.Avalonia.Launcher;
using Avalonia.Controls;
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
        /*
        options.ConfigureWindow = window =>
        {
            window.Title = "ASV Example Launcher";
            window.Width = 520;
            window.Height = 320;
            window.MinWidth = 420;
            window.MinHeight = 260;
            window.CanResize = true;
            window.Topmost = false;
            window.WindowDecorations = WindowDecorations.Full;
            window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        };
        */

        // Configure launcher behavior
        /*
        options.SuccessShutdownDelay = TimeSpan.FromSeconds(2);
        options.ShutdownOnSuccess = true;
        */

        // Configure only launcher content view
        /*
        options.CreateView = viewModel =>
            new DefaultLauncherView
            {
                DataContext = viewModel,
            };
        */
    }
}

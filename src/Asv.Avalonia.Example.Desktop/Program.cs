using System;
using Asv.Avalonia.Map;
using Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia.Example.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        using var app = AppHost
            .CreateBuilder()
            .UseLogToConsoleOnDebug()
            .UseAppPath(opt => opt.WithRelativeFolder("data"))
            .UseJsonUserConfig(opt =>
                opt.WithFileName("user_settings.json").WithAutoSave(TimeSpan.FromSeconds(1))
            )
            .UseAppInfo(opt => opt.FillFromAssembly(typeof(App).Assembly))
            .UseSoloRun(opt => opt.WithArgumentForwarding())
            .UseLogService(opt => opt.WithRelativeFolder("logs"))
            .UseAsvMap()
            .UsePluginManager(options =>
            {
                options.WithApiPackage("Asv.Drones.Gui.Api", "1.0.0");
                options.WithPluginPrefix("Asv.Drones.Gui.Plugin.");
            })
            .Build()
            .ExitIfNotFirstInstance();

        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            app.HandleApplicationCrash(e);
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions { OverlayPopups = true }) // Windows
            .With(new X11PlatformOptions { OverlayPopups = true, UseDBusFilePicker = false }) // Unix/Linux
            .With(new AvaloniaNativePlatformOptions { OverlayPopups = true }) // Mac
            .WithInterFont()
            .LogToTrace()
            .UseR3();
}

using System;
using System.Diagnostics;
using Asv.Cfg;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.Example.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        using var host = AppHost.Initialize(builder =>
        {
            builder
                .UseJsonConfig("config.json", true, TimeSpan.FromMilliseconds(500))
                .WithAppInfoFrom(typeof(App).Assembly)
                .UseLogging(options =>
                {
                    options.WithLogMinimumLevel<AppHostConfig>(cfg => cfg.LogMinLevel);
                    options.WithJsonLogFolder<AppHostConfig>("logs", cfg => cfg.RollingSizeKb);
#if DEBUG
                    options.WithLogToConsole();
#endif
                })
                .EnforceSingleInstance()
                .EnableArgumentForwarding()
                .WithArguments(args);
        });

        // If this is not the first instance, host have sent the arguments to the first instance and we can exit
        if (host.IsFirstInstance == false)
        {
            return;
        }

        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
        }
        catch (Exception e)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            Console.WriteLine(e);
            host.HandleApplicationCrash(e);
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

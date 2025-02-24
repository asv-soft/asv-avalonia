using System;
using System.Diagnostics;
using System.IO;
using Asv.Cfg;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Microsoft.Extensions.Configuration;
using Key = Avalonia.Remote.Protocol.Input.Key;

namespace Asv.Avalonia.Example.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
         using var app = AsvHost
             .CreateBuilder()
             .UseAppInfo()
             .UseSingleInstance(options => options.WithArgumentForwarding())
             .Build();

       // If this is not the first instance, host have sent the arguments to the first instance and we can exit
        app.ExitIfNotFirstInstance();

        var builder = AppHost.CreateBuilder();

        builder
            .UseJsonConfig("config.json", true, TimeSpan.FromMilliseconds(500))
            .SetAppInfoFrom(typeof(App).Assembly)
            .UseLogging(options =>
            {
                options.WithLogMinimumLevel<AppHostConfig>(cfg => cfg.LogMinLevel);
                options.WithJsonLogFolder<AppHostConfig>("logs", cfg => cfg.RollingSizeKb);
#if DEBUG
                options.WithLogToConsole();
#endif
            })
            .EnforceSingleInstance(options => options.EnableArgumentForwarding())
            .SetArguments(args);

        using var host = builder.Build();

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
            Console.WriteLine(e);
            host.HandleApplicationCrash(e);
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

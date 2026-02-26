using System;
using System.IO;
using System.Reflection;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Avalonia.Plugins;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
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
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
        }
        catch (Exception e)
        {
            AppHost.HandleApplicationCrash(e);
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions { OverlayPopups = true }) // Windows
            .With(new X11PlatformOptions { OverlayPopups = true, UseDBusFilePicker = false }) // Unix/Linux
            .With(new AvaloniaNativePlatformOptions { OverlayPopups = true }) // Mac
            .WithInterFont()
            .LogToTrace()
            .UseAsv(builder =>
            {
                builder
                    .UseDefault()
                    .UseAppInfo(opt => opt.FillFromAssembly(typeof(App).Assembly))
                    .UseAppPath(config => config.WithRelativeFolder("data"))
                    .UseJsonUserConfig(opt =>
                        opt.WithFileName("user_settings.json").WithAutoSave(TimeSpan.FromSeconds(1))
                    )
                    .UseSoloRun(opt => opt.WithArgumentForwarding())
                    .UseLogging(options =>
                    {
                        options.WithLogToFile(Path.Combine("data", "logs"));
                        options.WithLogToConsole();
                        options.WithLogViewer();
                        options.WithLogLevel(LogLevel.Trace);
                    })
                    .UseLogReaderService(new LogToFileOptions(Path.Combine("data", "logs")))
                    .UsePluginManager(options =>
                    {
                        options.WithApiPackage(
                            "Asv.Avalonia.Example.Api",
                            SemVersion.Parse("1.0.0")
                        );
                        options.WithPluginPrefix("Asv.Avalonia.Example.Plugin.");
                    })
                    .UseDesktopShell()
                    .UseModulePlugins()
                    .UseModuleGeoMap()
                    .UseModuleIo()
                    .UseExample();
            });
    }
}

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Asv.Avalonia.Example.Api;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Avalonia.Plugins;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
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
            var app = BuildAvaloniaApp();
            app.StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
            AppHost.Instance.StopAsync().GetAwaiter().GetResult();
            Task.Factory.StartNew(AppHost.Instance.Dispose).GetAwaiter().GetResult();
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
                    .UseOptionalLogViewer()
                    .UseOptionalSoloRun(opt =>
                        opt.WithArgumentForwarding().WithMutexName("Asv.Avalonia.Example.Desktop")
                    )
                    .UsePluginManager(options =>
                    {
                        options.WithApiPackage(
                            "Asv.Avalonia.Example.Api",
                            SemVersion.Parse("1.0.0")
                        );
                        options.WithPluginPrefix("Asv.Avalonia.Example.Plugin.");
                    })
                    .UseDesktopShell()
                    .UseModulePlugins(configure =>
                    {
                        configure
                            .WithApiPackage(typeof(Command1).Assembly)
                            .UseOptionalInstalled() // register installed plugins page
                            .UseOptionalMarket(); // register market plugins page
                    })
                    .UseModuleGeoMap()
                    .UseModuleIo()
                    .UseExampleApp();
            });
    }
}

using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Asv.Avalonia.Charts;
using Asv.Avalonia.Example.Api;
using Asv.Avalonia.GeoMap;
using Asv.Avalonia.IO;
using Asv.Avalonia.Launcher.Ready;
using Asv.Avalonia.Plugins;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging;
using Microsoft.Extensions.Logging;
using ZstdSharp.Unsafe;

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
                    .EnableLogging()
                    .RegisterCore(opt =>
                    {
                        opt.RegisterControls();
                        opt.RegisterServices(services =>
                        {
                            services
                                .RegisterSoloRun(solo =>
                                    solo.WithArgumentForwarding()
                                        .WithMutexName("Asv.Avalonia.Example.Desktop")
                                )
                                .RegisterAppArgsStore()
                                .RegisterAppInfo()
                                .RegisterAppPath()
                                .RegisterRestartFeature()
                                .RegisterDialogs()
                                .RegisterExtensions()
                                .RegisterFileAssociation()
                                .RegisterUnhandledExceptionsHandler()
                                .RegisterHotKeys()
                                .RegisterLocalizationService()
                                .RegisterLogViewer()
                                .RegisterLogToFile()
                                .RegisterSearchService()
                                .RegisterShellHost()
                                .RegisterThemeService()
                                .RegisterTimeProvider()
                                .RegisterUnitService()
                                .RegisterUserConfig()
                                .RegisterViewLocator();
                        });
                    })
                    .RegisterDesktopShell()
                    .RegisterModulePlugins(configure =>
                    {
                        configure.RegisterCore(core =>
                            core.RegisterServices(services =>
                            {
                                services.RegisterPluginBootloader(bootloader =>
                                    bootloader.WithApiPackage(typeof(Command1).Assembly)
                                );
                                services.RegisterPluginManager(options =>
                                {
                                    options.WithApiPackage(
                                        "Asv.Avalonia.Example.Api",
                                        SemVersion.Parse("1.0.0")
                                    );
                                    options.WithPluginPrefix("Asv.Avalonia.Example.Plugin.");
                                });
                            })
                        );
                        configure.RegisterShell(shell =>
                            shell.RegisterPages(pages =>
                                pages.RegisterSettings(settings =>
                                    settings
                                        .RegisterInstalled() // register installed plugins page
                                        .RegisterMarket()
                                )
                            )
                        );
                    })
                    .RegisterModuleGeoMap()
                    .RegisterModuleCharts()
                    .RegisterLauncher(cfg => cfg.IsOptional())
                    .RegisterModuleIo()
                    .RegisterExampleApp();
            });
    }
}

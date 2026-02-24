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
            var builder = AppHost.CreateBuilder(args);
            var dataFolder =
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

            builder
                .UseSystemTimeProvider()
                .UseUnhandledExceptionsHandler()
                .UseShellHost()
                .UseLayoutService()
                .UseAvalonia(BuildAvaloniaApp)
                .UseAppPath(opt => opt.WithRelativeFolder(Path.Combine(dataFolder, "data")))
                .UseJsonUserConfig(opt =>
                    opt.WithFileName("user_settings.json").WithAutoSave(TimeSpan.FromSeconds(1))
                )
                .UseAppInfo(opt => opt.FillFromAssembly(typeof(App).Assembly))
                .UseSoloRun(opt => opt.WithArgumentForwarding())
                .UseLogging(options =>
                {
                    options.WithLogToFile(Path.Combine(dataFolder, "data", "logs"));
                    options.WithLogToConsole();
                    options.WithLogViewer();
                    options.WithLogLevel(LogLevel.Trace);
                })
                .UseLogReaderService(new LogToFileOptions(Path.Combine(dataFolder, "data", "logs")))
                .UseModuleGeoMap()
                .UseModuleIo()
                .UsePluginManager(options =>
                {
                    options.WithApiPackage("Asv.Avalonia.Example.Api", SemVersion.Parse("1.0.0"));
                    options.WithPluginPrefix("Asv.Avalonia.Example.Plugin.");
                })
                .UseUnitService()
                .UseDefaultControls()
                .UseExtensions()
                .UseDesktopShell()
                .UseViewLocator()
                .UseThemeService()
                .UseSearchService()
                .UseNavigationService()
                .UseLocalizationService()
                .UseFileAssociation()
                .UseDialogs()
                .UseCommands()
                .UsePlugins()
                .UseExample();

            using var host = builder.Build();
            host.ExitIfNotFirstInstance();
            host.StartWithClassicDesktopLifetime(args, ShutdownMode.OnMainWindowClose);
        }
        catch (Exception e)
        {
            AppHost.HandleApplicationCrash(e);
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        if (Design.IsDesignMode)
        {
            var builder = AppHost.CreateBuilder([]);
            builder
                .UseInMemoryConfig()
                .UseDefaultControls()
                .UseUnitService()
                .UseNullExtension()
                .UseViewLocator()
                .UseDesingTimeThemeService()
                .UseDesignTimeSearchService()
                .UseDesignTimeShell()
                .UseDesignTimeNavigationService()
                .UseDesignTimeLogReaderService()
                .UseDesignTimeLocalizationService()
                .UseDesignTimeLayoutService()
                .UseDesignTimeFileAssociation()
                .UseDesignTimeDialogs()
                .UseDesignTimeCommands()
                .UseDesignTimeShellHost();

            builder.Build();
        }
        return AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .With(new Win32PlatformOptions { OverlayPopups = true }) // Windows
            .With(new X11PlatformOptions { OverlayPopups = true, UseDBusFilePicker = false }) // Unix/Linux
            .With(new AvaloniaNativePlatformOptions { OverlayPopups = true }) // Mac
            .WithInterFont()
            .LogToTrace()
            .UseR3(x =>
                AppHost
                    .Instance.Services.GetRequiredService<IUnhandledExceptionHandler>()
                    .R3UnhandledException(x)
            );
    }
}

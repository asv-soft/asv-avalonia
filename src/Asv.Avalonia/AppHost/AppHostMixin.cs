using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public static class AppHostMixin
{
    public const string DesignTimeEnvironmentName = "Design";

    extension(AppBuilder avaloniaBuilder)
    {
        public AppBuilder UseAsv(Action<AppHost.Builder> configure)
        {
            // this is for unhandled exception from R3 library
            avaloniaBuilder.UseR3(x =>
                AppHost
                    .Instance.Services.GetService<IUnhandledExceptionHandler>()
                    ?.R3UnhandledException(x)
            );

            var asvBuilder = AppHost.CreateBuilder();
            if (Design.IsDesignMode)
            {
                asvBuilder.Environment.EnvironmentName = DesignTimeEnvironmentName;
            }
            configure(asvBuilder);
            if (Design.IsDesignMode)
            {
                asvBuilder.ReplaceForDesignTime();
            }
            avaloniaBuilder.AfterPlatformServicesSetup(_ => asvBuilder.Build());
            return avaloniaBuilder;
        }
    }
    extension(IHostApplicationBuilder builder)
    {
        public bool IsDesignTimeEnvironment =>
            builder.Environment.EnvironmentName == DesignTimeEnvironmentName;

        public IHostApplicationBuilder UseDefault()
        {
            builder.Logging.ClearProviders();
            return builder
                .UseTimeProvider()
                .UseThemeService()
                .UseSearchService()
                .UseNavigationService()
                .UseLocalizationService()
                .UseExtensions()
                .UseShellHost()
                .UseViewLocator()
                .UseLayoutService()
                .UseDefaultControls()
                .UseUnitService()
                .UseFileAssociation()
                .UseDialogs()
                .UseCommands()
                .UseUnhandledExceptionsHandler();
        }

        internal IHostApplicationBuilder ReplaceForDesignTime()
        {
            builder
                .UseNullExtension()
                .UseDesingTimeThemeService()
                .UseDesignTimeSearchService()
                .UseDesignTimeShell()
                .UseDesignTimeNavigationService()
                .UseDesignTimeLogReaderService()
                .UseDesignTimeLocalizationService()
                .UseDesignTimeLayoutService()
                .UseDesignTimeFileAssociation()
                .UseDesignTimeDialogs()
                .UseDesignTimeCommands();
            return builder;
        }

        internal IHostApplicationBuilder UseTimeProvider()
        {
            // same type for design and runtime
            builder.Services.AddSingleton(TimeProvider.System);
            return builder;
        }
    }
}

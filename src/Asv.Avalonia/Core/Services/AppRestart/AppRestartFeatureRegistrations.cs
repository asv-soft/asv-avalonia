using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class AppRestartFeatureRegistrations
{
    extension(ServicesRegistrations.Builder builder)
    {
        public ServicesRegistrations.Builder RegisterRestartFeature()
        {
            if (builder.AppBuilder.IsDesignTimeEnvironment)
            {
                builder.AppBuilder.Services.AddSingleton<IAppRestartScheduler>(
                    NullAppRestartScheduler.Instance
                );
                return builder;
            }

            builder.AppBuilder.Services.AddSingleton<IAppRestartScheduler, AppRestartScheduler>();
            builder.AppBuilder.Services.AddSingleton(CreateRestartFeature);
            builder.AppBuilder.Services.AddHostedService<AppRestartExecutor>();
            return builder;
        }
    }

    private static IAppRestartFeature CreateRestartFeature(IServiceProvider provider)
    {
        if (IsDesktopEnvironment())
        {
            return ActivatorUtilities.CreateInstance<DesktopAppRestartFeature>(provider);
        }

        if (OperatingSystem.IsAndroid())
        {
            // TODO: implement
        }

        if (OperatingSystem.IsIOS())
        {
            // TODO: implement
        }

        return ActivatorUtilities.CreateInstance<NullAppRestartFeature>(provider);
    }

    private static bool IsDesktopEnvironment()
    {
        return OperatingSystem.IsWindows()
            || OperatingSystem.IsMacOS()
            || OperatingSystem.IsLinux()
            || OperatingSystem.IsFreeBSD();
    }
}

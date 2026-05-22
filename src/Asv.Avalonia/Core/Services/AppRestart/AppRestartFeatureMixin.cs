using DotNext.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class AppRestartFeatureMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseRestartFeature()
        {
            if (builder.IsDesignTimeEnvironment)
            {
                builder.Services.AddSingleton<IAppRestartScheduler>(
                    NullAppRestartScheduler.Instance
                );
                return builder;
            }

            builder.Services.AddSingleton<IAppRestartScheduler, AppRestartScheduler>();
            builder.Services.AddSingleton(CreateRestartFeature);
            builder.Services.AddHostedService<AppRestartExecutor>();
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

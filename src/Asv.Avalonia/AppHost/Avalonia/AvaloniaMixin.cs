using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class AvaloniaMixin
{
    public static IHostApplicationBuilder UseAvalonia(
        this IHostApplicationBuilder builder,
        Func<AppBuilder> configureApp
    )
    {
        builder.Services.AddSingleton(typeof(IHostLifetime), new AvaloniaLifetime(configureApp));
        return builder;
    }

    public static void StartWithClassicDesktopLifetime(
        this IHost host,
        string[] args,
        ShutdownMode shutdownMode
    )
    {
        host.Start();
        if (host.Services.GetService<IHostLifetime>() is AvaloniaLifetime avaloniaLifeTime)
        {
            avaloniaLifeTime.AppBuilder.StartWithClassicDesktopLifetime(args, shutdownMode);
        }

        host.StopAsync().GetAwaiter().GetResult();
    }
}

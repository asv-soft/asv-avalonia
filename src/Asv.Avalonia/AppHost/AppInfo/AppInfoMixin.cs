using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class AppInfoMixin
{
    public static IHostApplicationBuilder UseAppInfo(
        this IHostApplicationBuilder builder,
        Action<AppInfoBuilder>? configure = null
    )
    {
        var infoBuilder = new AppInfoBuilder();
        configure?.Invoke(infoBuilder);
        builder.Services.AddSingleton(infoBuilder.Build());
        return builder;
    }
}

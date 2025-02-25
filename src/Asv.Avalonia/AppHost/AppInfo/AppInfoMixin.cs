using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class AppInfoMixin
{
    public static AppHostBuilder UseAppInfo(
        this AppHostBuilder builder,
        Action<AppInfoBuilder>? configure = null
    )
    {
        var infoBuilder = new AppInfoBuilder();
        configure?.Invoke(infoBuilder);
        builder.Services.AddSingleton(infoBuilder.Build());
        return builder;
    }
}

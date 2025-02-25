using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class AppPathMixin
{
    public static AppHostBuilder UseAppPath(
        this AppHostBuilder builder,
        Action<AppPathBuilder>? configure = null
    )
    {
        var pathBuilder = new AppPathBuilder();
        configure?.Invoke(pathBuilder);
        var options = builder
            .Services.AddSingleton<IAppPath, AppPath>()
            .AddOptions<AppPathConfig>()
            .Bind(builder.AppConfig.GetSection(AppPathConfig.Section));
        pathBuilder.Build(options);
        return builder;
    }
}

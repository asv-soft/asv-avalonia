using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class AppPathMixin
{
    public static IHostApplicationBuilder UseAppPath(
        this IHostApplicationBuilder builder,
        Action<AppPathBuilder>? configure = null
    )
    {
        var pathBuilder = new AppPathBuilder();
        configure?.Invoke(pathBuilder);
        var options = builder
            .Services.AddSingleton<IAppPath, AppPath>()
            .AddOptions<AppPathConfig>()
            .Bind(builder.Configuration.GetSection(AppPathConfig.Section));
        pathBuilder.Build(options);
        return builder;
    }
}

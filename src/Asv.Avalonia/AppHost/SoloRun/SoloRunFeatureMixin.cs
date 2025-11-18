using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class SoloRunFeatureMixin
{
    public static IHostApplicationBuilder UseSoloRun(
        this IHostApplicationBuilder builder,
        Action<SoloRunFeatureBuilder>? configure = null
    )
    {
        var options = builder
            .Services.AddSingleton<ISoloRunFeature, SoloRunFeature>()
            .AddHostedService(x => x.GetRequiredService<ISoloRunFeature>())
            .AddOptions<SoloRunFeatureOptions>()
            .Bind(builder.Configuration.GetSection(SoloRunFeatureOptions.Section))
            .PostConfigure<IAppInfo>(
                (config, info) =>
                {
                    config.Mutex ??= info.Name;
                    config.Pipe ??= info.Name;
                }
            );

        var subBuilder = new SoloRunFeatureBuilder();
        configure?.Invoke(subBuilder);
        subBuilder.Build(options);
        return builder;
    }

    public static ISoloRunFeature GetSoloRunFeature(this IHost host)
    {
        return host.Services.GetRequiredService<ISoloRunFeature>();
    }

    public static IHost ExitIfNotFirstInstance(this IHost host)
    {
        if (host.GetSoloRunFeature().IsFirstInstance == false)
        {
            Environment.Exit(0);
        }

        return host;
    }
}

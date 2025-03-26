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
            .AddOptions<SoloRunFeatureConfig>()
            .Bind(builder.Configuration.GetSection(SoloRunFeatureConfig.Section))
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

    public static ISoloRunFeature GetSoloRunFeature(this AppHost host)
    {
        return host.GetService<ISoloRunFeature>();
    }

    public static AppHost ExitIfNotFirstInstance(this AppHost host)
    {
        if (host.GetSoloRunFeature().IsFirstInstance == false)
        {
            Environment.Exit(0);
        }

        return host;
    }
}

using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class SoloRunFeatureMixin
{
    public static AppHostBuilder UseSoloRun(
        this AppHostBuilder builder,
        Action<SoloRunFeatureBuilder>? configure = null
    )
    {
        var options = builder
            .Services.AddSingleton<ISoloRunFeature, SoloRunFeature>()
            .AddOptions<SoloRunFeatureConfig>()
            .Bind(builder.AppConfig.GetSection(SoloRunFeatureConfig.Section))
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
        return host.Services.GetRequiredService<ISoloRunFeature>();
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

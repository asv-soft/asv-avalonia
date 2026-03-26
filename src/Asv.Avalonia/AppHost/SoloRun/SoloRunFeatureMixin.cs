using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class SoloRunFeatureMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseOptionalSoloRun(
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

        public IHostApplicationBuilder UseDesignTimeOptionalSoloRun()
        {
            builder.Services.ReplaceSingleton(NullSoloRunFeature.Instance);
            return builder;
        }
    }
}

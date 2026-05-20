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
            if (builder.IsDesignTimeEnvironment)
            {
                builder.UseDesignTimeOptionalSoloRun();
                return builder;
            }

            var options = builder
                .Services.AddHostedService<SoloRunFeature>()
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

        private void UseDesignTimeOptionalSoloRun()
        {
            builder.Services.ReplaceSingleton<IAppArgsStore, AppArgsStore>();
            builder.Services.AddHostedService(_ => NullSoloRunFeature.Instance);
        }
    }
}

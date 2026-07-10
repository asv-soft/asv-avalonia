using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class SoloRunFeatureRegistrations
{
    extension(ServicesRegistrations.Builder builder)
    {
        public ServicesRegistrations.Builder RegisterSoloRun(
            Action<SoloRunFeatureBuilder>? configure = null
        )
        {
            if (builder.AppBuilder.IsDesignTimeEnvironment)
            {
                builder.RegisterDesignTimeOptionalSoloRun();
                return builder;
            }

            var options = builder
                .AppBuilder.Services.AddHostedService<SoloRunFeature>()
                .AddOptions<SoloRunFeatureOptions>()
                .Bind(builder.AppBuilder.Configuration.GetSection(SoloRunFeatureOptions.Section))
                .PostConfigure<IHostEnvironment>(
                    (config, environment) =>
                    {
                        config.Mutex = GetDefaultName(config.Mutex, environment);
                        if (config.ArgForward)
                        {
                            config.Pipe = GetDefaultName(config.Pipe, environment);
                        }
                    }
                );

            var subBuilder = new SoloRunFeatureBuilder();
            configure ??= b => b.UseDefault();
            configure.Invoke(subBuilder);
            subBuilder.Build(options);
            return builder;
        }

        private void RegisterDesignTimeOptionalSoloRun()
        {
            builder.AppBuilder.Services.AddHostedService(_ => NullSoloRunFeature.Instance);
        }

        private static string GetDefaultName(string? configuredName, IHostEnvironment environment)
        {
            if (!string.IsNullOrWhiteSpace(configuredName))
            {
                return configuredName;
            }

            if (!string.IsNullOrWhiteSpace(environment.ApplicationName))
            {
                return environment.ApplicationName;
            }

            return Path.GetFileNameWithoutExtension(Environment.ProcessPath)
                ?? typeof(SoloRunFeature).Assembly.GetName().Name ?? nameof(SoloRunFeature);
        }
    }
}

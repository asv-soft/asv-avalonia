using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia.Launcher.Ready;

public class LauncherFeatureOptions
{
    public const string Section = "Launcher";
    public bool IsOptional { get; set; }
}

public static class LauncherReadyMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseLauncher(Action<Builder>? configure = null)
        {
            var options = builder
                .Services.AddOptions<LauncherFeatureOptions>()
                .Bind(builder.Configuration.GetSection(LauncherFeatureOptions.Section));

            var launcherBuilder = new Builder(builder, options);
            launcherBuilder.RegisterDefault();
            configure?.Invoke(launcherBuilder);
            return builder;
        }

        public Builder ModuleLauncherReady =>
            new(
                builder,
                builder
                    .Services.AddOptions<LauncherFeatureOptions>()
                    .Bind(builder.Configuration.GetSection(LauncherFeatureOptions.Section))
            );
    }

    public class Builder(
        IHostApplicationBuilder builder,
        OptionsBuilder<LauncherFeatureOptions> options
    )
    {
        public IHostApplicationBuilder Parent => builder;

        public Builder IsOptional(bool isOptional = true)
        {
            options.Configure(x => x.IsOptional = isOptional);
            return this;
        }

        public IHostApplicationBuilder RegisterDefault()
        {
            builder.Services.AddSingleton<ILauncherNotifier, LauncherNotifier>();
            builder.Services.AddHostedService<LauncherFeature>();
            return builder;
        }
    }
}

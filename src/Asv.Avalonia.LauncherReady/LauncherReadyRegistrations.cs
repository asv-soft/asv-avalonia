using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia.Launcher.Ready;

public class LauncherFeatureOptions
{
    public const string Section = "Launcher";
    public bool IsOptional { get; set; }
}

public static class LauncherReadyRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder RegisterLauncher(Action<Builder>? configure = null)
        {
            var options = builder
                .Services.AddOptions<LauncherFeatureOptions>()
                .Bind(builder.Configuration.GetSection(LauncherFeatureOptions.Section));

            var launcherBuilder = new Builder(builder, options);

            if (!builder.IsDesignTimeEnvironment)
            {
                builder.Services.AddSingleton<ILauncherNotifier, LauncherNotifier>();
                builder.Services.AddHostedService<LauncherFeature>();
            }
            else
            {
                builder.RegisterDesignTimeLauncher();
            }

            configure?.Invoke(launcherBuilder);

            builder.ChangeAppRestartForDesktop();
            return builder;
        }

        public Builder ModuleLauncherReady =>
            new(
                builder,
                builder
                    .Services.AddOptions<LauncherFeatureOptions>()
                    .Bind(builder.Configuration.GetSection(LauncherFeatureOptions.Section))
            );

        private IHostApplicationBuilder ChangeAppRestartForDesktop()
        {
            builder.Services.ReplaceSingleton<
                IAppRestartFeature,
                DesktopWithLauncherAppRestartFeature
            >();
            return builder;
        }

        private void RegisterDesignTimeLauncher()
        {
            builder.Services.AddSingleton<ILauncherNotifier, NullLauncherNotifier>();
            builder.Services.AddHostedService<LauncherFeature>();
        }
    }

    public class Builder(
        IHostApplicationBuilder builder,
        OptionsBuilder<LauncherFeatureOptions> options
    )
    {
        public IHostApplicationBuilder AppBuilder => builder;

        public Builder IsOptional(bool isOptional = true)
        {
            options.Configure(x => x.IsOptional = isOptional);
            return this;
        }
    }
}

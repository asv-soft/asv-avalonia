using System.IO.Abstractions;
using Asv.Cfg;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class UserConfigMixin
{
    public static AppHostBuilder UseJsonUserConfig(
        this AppHostBuilder builder,
        Action<UserJsonConfigurationBuilder>? configure = null
    )
    {
        var config = new UserJsonConfigurationBuilder();
        configure?.Invoke(config);
        var options = builder
            .Services.AddSingleton<IConfiguration, UserJsonConfiguration>()
            .AddOptions<UserConfigurationConfig>()
            .Bind(builder.AppConfig);
        config.Build(options);
        return builder;
    }
}

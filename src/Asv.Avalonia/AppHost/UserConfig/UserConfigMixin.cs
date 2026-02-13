using Asv.Cfg;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class UserConfigMixin
{
    public static IHostApplicationBuilder UseInMemoryConfig(this IHostApplicationBuilder builder)
    {
        var config = new InMemoryConfiguration();
        builder.Services.AddSingleton<IConfiguration, InMemoryConfiguration>();
        return builder;
    }

    public static IHostApplicationBuilder UseJsonUserConfig(
        this IHostApplicationBuilder builder,
        Action<UserJsonConfigurationBuilder>? configure = null
    )
    {
        var config = new UserJsonConfigurationBuilder();
        configure?.Invoke(config);
        var options = builder
            .Services.AddSingleton<IConfiguration, UserJsonConfiguration>()
            .AddOptions<UserConfigurationOptions>()
            .Bind(builder.Configuration);
        config.Build(options);
        return builder;
    }
}

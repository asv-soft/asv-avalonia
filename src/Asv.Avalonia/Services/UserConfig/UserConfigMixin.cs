using Asv.Cfg;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class UserConfigMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseJsonUserConfig(Action<Builder>? configure = null)
        {
            configure ??= (b) => b.UseDefault();
            configure(new Builder(builder));
            return builder;
        }
    }

    public class Builder(IHostApplicationBuilder builder)
    {
        public void UseDefault()
        {
            UseJsonConfig();
        }

        public Builder UseJsonConfig()
        {
            builder
                .Services.AddSingleton<IConfiguration, UserJsonConfiguration>()
                .AddOptions<UserConfigurationOptions>()
                .BindConfiguration(UserConfigurationOptions.SectionName);
            return this;
        }

        public Builder UseInMemoryConfig()
        {
            builder.Services.AddSingleton<IConfiguration, InMemoryConfiguration>();
            return this;
        }
    }
}

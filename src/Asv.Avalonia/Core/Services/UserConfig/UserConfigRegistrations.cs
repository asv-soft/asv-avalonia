using Asv.Cfg;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class UserConfigRegistrations
{
    extension(ServicesRegistrations.Builder builder)
    {
        public Builder UserConfig => new(builder);

        public ServicesRegistrations.Builder RegisterUserConfig(Action<Builder>? configure = null)
        {
            configure ??= (b) => b.RegisterDefault();
            configure(new Builder(builder));
            return builder;
        }
    }

    public class Builder(ServicesRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterDefault()
        {
            RegisterJsonConfig();
            return this;
        }

        public Builder RegisterJsonConfig()
        {
            builder
                .AppBuilder.Services.AddSingleton<IConfiguration, UserJsonConfiguration>()
                .AddOptions<UserConfigurationOptions>()
                .BindConfiguration(UserConfigurationOptions.SectionName);
            return this;
        }

        public Builder RegisterInMemoryConfig()
        {
            builder.AppBuilder.Services.AddSingleton<IConfiguration, InMemoryConfiguration>();
            return this;
        }
    }
}

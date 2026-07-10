using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class TimeProviderRegistrations
{
    extension(ServicesRegistrations.Builder builder)
    {
        public Builder TimeProvider => new(builder);

        public ServicesRegistrations.Builder RegisterTimeProvider(Action<Builder>? configure = null)
        {
            configure ??= x => x.UseDefault();
            var infoBuilder = new Builder(builder);
            configure(infoBuilder);
            return builder;
        }
    }

    public class Builder(ServicesRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder UseDefault()
        {
            WithSystemProvider();
            return this;
        }

        public Builder WithSystemProvider()
        {
            builder.AppBuilder.Services.AddSingleton(TimeProvider.System);
            return this;
        }

        public Builder WithCustomProvider(TimeProvider provider)
        {
            builder.AppBuilder.Services.AddSingleton(provider);
            return this;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class ExtensionsMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseExtensions()
        {
            builder.Services.AddSingleton<IExtensionService, ExtensionService>();
            return builder;
        }

        public IHostApplicationBuilder UseNullExtension()
        {
            builder.Services.AddSingleton(NullExtensionService.Instance);
            return builder;
        }

        public Builder Extensions => new(builder);
    }

    public class Builder(IHostApplicationBuilder builder)
    {
        public Builder Register<TContext, TExtension>()
            where TExtension : class, IExtensionFor<TContext>
        {
            builder.Services.AddTransient<IExtensionFor<TContext>, TExtension>();
            return this;
        }

        public Builder Register<TContext, TExtension>(string key)
            where TExtension : class, IExtensionFor<TContext>
        {
            builder.Services.AddKeyedTransient<IExtensionFor<TContext>, TExtension>(key);
            return this;
        }
    }
}

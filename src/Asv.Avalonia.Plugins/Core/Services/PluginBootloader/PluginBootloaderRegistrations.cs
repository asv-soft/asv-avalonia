using System.Reflection;
using Asv.Avalonia;
using Asv.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia.Plugins;

public static class PluginBootloaderRegistrations
{
    extension(ServicesRegistrations.Builder builder)
    {
        public ServicesRegistrations.Builder RegisterPluginBootloader(
            Action<Builder>? configure = null
        )
        {
            var options =
                builder
                    .AppBuilder.Configuration.GetSection(PluginBootloaderOptions.SectionName)
                    .Get<PluginBootloaderOptions>()
                ?? new PluginBootloaderOptions();
            configure?.Invoke(new Builder(builder, options));

            var loader = new PluginBootloader(
                Options.Create(options),
                builder.AppBuilder.Environment
            );
            builder.AppBuilder.Services.AddSingleton<IPluginBootloader>(loader);
            builder.AppBuilder.AddPostConfigureCallbacks(loader.InitPlugins);
            return builder;
        }
    }

    public class Builder(ServicesRegistrations.Builder builder, PluginBootloaderOptions options)
        : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public PluginBootloaderOptions Options => options;

        public Builder WithApiPackage(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            Options.ApiPackageName =
                assembly.GetName().Name ?? throw new InvalidOperationException();
            var attributes = assembly.GetCustomAttributes(
                typeof(AssemblyInformationalVersionAttribute),
                false
            );

            ArgumentNullException.ThrowIfNull(attributes);
            if (attributes.Length == 0)
            {
                throw new ArgumentNullException(nameof(attributes));
            }

            var nameAttribute = (AssemblyInformationalVersionAttribute)attributes[0];
            ArgumentException.ThrowIfNullOrEmpty(nameAttribute.InformationalVersion);

            Options.ApiVersion = SemVersion.Parse(nameAttribute.InformationalVersion).ToString();

            return this;
        }
    }
}

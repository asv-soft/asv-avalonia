using System.Reflection;
using Asv.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia.Plugins;

public static class PluginManagerRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder PluginManager => builder.ModulePlugins.Core.Services.PluginManager;
    }

    extension(ServicesRegistrations.Builder builder)
    {
        public Builder PluginManager => new(builder);

        public ServicesRegistrations.Builder RegisterPluginManager(
            Action<Builder>? configure = null
        )
        {
            var subBuilder = new Builder(builder);
            configure?.Invoke(subBuilder);
            return builder;
        }
    }

    public class Builder : IDependencyBuilder
    {
        private readonly ServicesRegistrations.Builder _builder;
        private string _apiPackageName = string.Empty;
        private SemVersion _apiVersion = "0.0.0";
        private string _nugetPluginPrefix = string.Empty;
        private readonly List<PluginServer> _servers =
        [
            new("NuGet", "https://api.nuget.org/v3/index.json"),
        ];
        private string _relativePluginFolder = "plugins";
        private string _relativeNugetFolder = "nuget";
        private string _relativeNugetCacheFolder = "nuget_cache";
        private string _salt = "Asv.Avalonia.Plugins";

        public Builder(ServicesRegistrations.Builder builder)
        {
            _builder = builder;
            var options = builder
                .AppBuilder.Services.AddOptions<PluginManagerOptions>()
                .Bind(builder.AppBuilder.Configuration.GetSection(PluginManagerOptions.Section));

            builder.AppBuilder.Services.TryAddSingleton<IPluginManager, PluginManager>();
            Build(options);
        }

        public IHostApplicationBuilder AppBuilder => _builder.AppBuilder;

        public Builder WithApiPackage(string apiPackageName, SemVersion apiVersion)
        {
            ArgumentNullException.ThrowIfNull(apiPackageName);
            ArgumentNullException.ThrowIfNull(apiVersion);
            _apiPackageName = apiPackageName;
            _apiVersion = apiVersion;
            return this;
        }

        public Builder WithApiPackage(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            _apiPackageName = assembly.GetName().Name ?? throw new InvalidOperationException();
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

            _apiVersion = SemVersion.Parse(nameAttribute.InformationalVersion);

            return this;
        }

        public Builder WithPluginPrefix(string pluginPrefix)
        {
            ArgumentNullException.ThrowIfNull(pluginPrefix);
            _nugetPluginPrefix = pluginPrefix;
            return this;
        }

        public Builder WithServer(PluginServer server)
        {
            ArgumentNullException.ThrowIfNull(server);
            if (_servers.Find(l => l.SourceUri == server.SourceUri) == null)
            {
                _servers.Add(server);
            }

            return this;
        }

        public Builder WithRelativePluginFolder(string pluginFolder)
        {
            ArgumentNullException.ThrowIfNull(pluginFolder);
            _relativePluginFolder = pluginFolder;
            return this;
        }

        public Builder WithRelativeNugetFolder(string nugetFolder)
        {
            ArgumentNullException.ThrowIfNull(nugetFolder);
            _relativeNugetFolder = nugetFolder;
            return this;
        }

        public Builder WithRelativeNugetCacheFolder(string nugetCacheFolder)
        {
            ArgumentNullException.ThrowIfNull(nugetCacheFolder);
            _relativeNugetCacheFolder = nugetCacheFolder;
            return this;
        }

        public Builder WithSalt(string salt)
        {
            ArgumentNullException.ThrowIfNull(salt);
            _salt = salt;
            return this;
        }

        private OptionsBuilder<PluginManagerOptions> Build(
            OptionsBuilder<PluginManagerOptions> options
        )
        {
            return options.Configure(
                (PluginManagerOptions config, IHostEnvironment path) =>
                {
                    config.PluginDirectory = Path.Combine(
                        path.ContentRootPath,
                        _relativePluginFolder
                    );
                    config.NugetDirectory = Path.Combine(
                        path.ContentRootPath,
                        _relativeNugetFolder
                    );
                    config.NugetCacheDirectory = Path.Combine(
                        path.ContentRootPath,
                        _relativeNugetCacheFolder
                    );
                    config.ApiVersion = _apiVersion.ToString();
                    config.ApiPackageId = _apiPackageName;
                    config.NugetPluginPrefix = _nugetPluginPrefix;
                    config.Salt = _salt;
                    config.DefaultServers = _servers;
                }
            );
        }
    }
}

using Asv.Common;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia;

public class PluginManagerBuilder
{
    private string _apiPackageName = string.Empty;
    private SemVersion _apiVersion = "0.0.0";
    private string _nugetPluginPrefix = string.Empty;
    private readonly List<PluginServer> _servers = [];

    internal PluginManagerBuilder() { }

    /// <summary>
    /// Sets the API package name for the Plugin Manager that an application uses as a shared package, linking plugins.
    /// </summary>
    /// <param name="apiPackageName">The API package name to be set.</param>
    /// <param name="apiVersion">The API version to be set.</param>
    public PluginManagerBuilder WithApiPackage(string apiPackageName, SemVersion apiVersion)
    {
        ArgumentNullException.ThrowIfNull(apiPackageName);
        ArgumentNullException.ThrowIfNull(apiVersion);
        _apiPackageName = apiPackageName;
        _apiVersion = apiVersion;

        return this;
    }

    /// <summary>
    /// Sets the plugin package prefix that will be used as a NuGet search filter, e.g. "Asv.Avalonia.Plugin.".
    /// </summary>
    /// <param name="pluginPrefix">The NuGet package prefix to be set.</param>
    public PluginManagerBuilder WithPluginPrefix(string pluginPrefix)
    {
        ArgumentNullException.ThrowIfNull(pluginPrefix);
        _nugetPluginPrefix = pluginPrefix;

        return this;
    }

    /// <summary>
    /// Adds a NuGet plugins server to the Plugin Manager's list of servers if it doesn't already exist.
    /// </summary>
    /// <param name="server">The NuGet plugins server to be added.</param>
    public PluginManagerBuilder WithServer(PluginServer server)
    {
        ArgumentNullException.ThrowIfNull(server);
        if (_servers.Find(l => l.SourceUri == server.SourceUri) == null)
        {
            _servers.Add(server);
        }

        return this;
    }

    internal OptionsBuilder<BuilderPluginManagerConfig> Build(
        OptionsBuilder<BuilderPluginManagerConfig> options
    )
    {
        return options.Configure(config =>
        {
            config.ApiVersion = _apiVersion;
            config.ApiPackageName = _apiPackageName;
            config.NugetPluginPrefix = _nugetPluginPrefix;
        });
    }
}

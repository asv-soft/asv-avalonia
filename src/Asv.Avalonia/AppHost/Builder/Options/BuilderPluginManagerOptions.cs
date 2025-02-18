using Asv.Common;

namespace Asv.Avalonia;

public sealed class BuilderPluginManagerOptions
{
    public string ApiPackageName { get; set; } = string.Empty;
    public SemVersion ApiVersion { get; set; } = "0.0.0";
    public string NugetPluginPrefix { get; set; } = string.Empty;
    public List<PluginServer> Servers { get; } = [];
}

public static class BuilderPluginManagerExtensions
{
    /// <summary>
    /// Sets the API package name for the Plugin Manager that an application uses as a shared package, linking plugins.
    /// </summary>
    /// <param name="options">The plugin manager options instance.</param>
    /// <param name="apiPackageName">The API package name to be set.</param>
    /// <param name="apiVersion">The API version to be set.</param>
    public static void WithApiPackage(
        this BuilderPluginManagerOptions options,
        string apiPackageName,
        SemVersion apiVersion
    )
    {
        ArgumentNullException.ThrowIfNull(apiPackageName);
        ArgumentNullException.ThrowIfNull(apiVersion);
        options.ApiPackageName = apiPackageName;
        options.ApiVersion = apiVersion;
    }

    /// <summary>
    /// Sets the plugin package prefix that will be used as a NuGet search filter, e.g. "Asv.Avalonia.Plugin.".
    /// </summary>
    /// <param name="options">The plugin manager options instance.</param>
    /// <param name="pluginPrefix">The NuGet package prefix to be set.</param>
    public static void WithPluginPrefix(
        this BuilderPluginManagerOptions options,
        string pluginPrefix
    )
    {
        ArgumentNullException.ThrowIfNull(pluginPrefix);
        options.NugetPluginPrefix = pluginPrefix;
    }

    /// <summary>
    /// Adds a NuGet plugins server to the Plugin Manager's list of servers if it doesn't already exist.
    /// </summary>
    /// <param name="options">The plugin manager options instance.</param>
    /// <param name="server">The NuGet plugins server to be added.</param>
    public static void WithServer(this BuilderPluginManagerOptions options, PluginServer server)
    {
        ArgumentNullException.ThrowIfNull(server);
        if (options.Servers.Find(l => l.SourceUri == server.SourceUri) == null)
        {
            options.Servers.Add(server);
        }
    }
}

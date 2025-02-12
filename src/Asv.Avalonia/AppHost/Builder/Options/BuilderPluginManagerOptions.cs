using Asv.Common;

namespace Asv.Avalonia;

public sealed class BuilderPluginManagerOptions
{
    public string ApiPackageName { get; set; } = string.Empty;
    public SemVersion ApiVersion { get; set; } = "0.0.0";
    public string NugetPluginPrefix { get; set; } = string.Empty;
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
    /// Sets the plugin package prefix that will be used as a NuGet search filter, e.g. "Asv.Avalonia.Plugin."
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
}

namespace Asv.Avalonia;

public static class AppBuilderPluginManagerExtensions
{
    /// <summary>
    /// Adds the Plugin Manager to the app core and configures it in the application builder.
    /// </summary>
    /// <param name="builder">The AppHostBuilder to add the service to.</param>
    /// <param name="options">Options to set up the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IAppHostBuilder UsePluginManager(
        this IAppHostBuilder builder,
        BuilderPluginManagerOptions options
    )
    {
        var pm = new PluginManager(
            builder.Core.Services,
            builder.Core.AppFolder,
            options.ApiPackageName,
            options.NugetPluginPrefix,
            options.ApiVersion,
            builder.Core.Configuration,
            builder.Core.LogService
        );
        options.Servers.ForEach(s => pm.AddServer(s));
        builder.Core.Services.WithExport<IPluginManager>(pm);
        return builder;
    }

    /// <summary>
    /// Adds the Plugin Manager to the app core and configures it in the application builder.
    /// </summary>
    /// <param name="builder">The AppHostBuilder to add the service to.</param>
    /// <param name="setupAction">Action to set up the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IAppHostBuilder UsePluginManager(
        this IAppHostBuilder builder,
        Action<BuilderPluginManagerOptions>? setupAction = null
    )
    {
        var options = new BuilderPluginManagerOptions();

        setupAction?.Invoke(options);

        return builder.UsePluginManager(options);
    }
}

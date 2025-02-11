using Asv.Cfg;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class AppBuilderConfigurationExtensions
{
    /// <summary>
    /// !!! Must be used before other options !!!
    /// Configures the application host builder with the specified configuration implementation.
    /// </summary>
    /// <param name="builder">The AppHostBuilder to add the configuration to.</param>
    /// <param name="fileName">The name of the config file.</param>
    /// <param name="createIfNotExist">Option that defines whether to create the file if it does not exist.</param>
    /// <param name="flushToFileDelayMs">The delay of the flush to file process.</param>
    /// <returns>The current instance of the application host builder.</returns>
    public static IAppHostBuilder UseJsonConfig(
        this IAppHostBuilder builder,
        string fileName,
        bool createIfNotExist,
        TimeSpan? flushToFileDelayMs
    )
    {
        var cfg = new JsonOneFileConfiguration(fileName, createIfNotExist, flushToFileDelayMs);
        builder.Core.Configuration = cfg;
        return builder;
    }

    /// <summary>
    /// Configures the application host builder with the specified configuration implementation.
    /// </summary>
    /// <param name="builder">The AppHostBuilder to add the configuration to.</param>
    /// <param name="configuration">The configuration instance to be used by the application.</param>
    /// <returns>The current instance of the application host builder.</returns>
    public static IAppHostBuilder UseConfig(
        this IAppHostBuilder builder,
        IConfiguration configuration
    )
    {
        builder.Core.Configuration = configuration;
        return builder;
    }
}

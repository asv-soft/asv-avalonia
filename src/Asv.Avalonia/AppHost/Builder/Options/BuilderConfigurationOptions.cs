using System.Reflection;
using Asv.Cfg;

namespace Asv.Avalonia;

public class BuilderConfigurationOptions
{
    public required Func<IConfiguration> CreateConfigCallback { get; set; }
}

public static class BuilderConfigurationOptionsExtensions
{
    /// <summary>
    /// Configures the application host builder with the specified configuration implementation.
    /// </summary>
    /// <param name="configuration">The configuration instance to be used by the application.</param>
    /// <returns>The current instance of the application host builder.</returns>
    public static BuilderConfigurationOptions WithConfiguration(
        this BuilderConfigurationOptions options,
        IConfiguration configuration
    )
    {
        options.CreateConfigCallback = () => configuration;
        return options;
    }

    /// <summary>
    /// Configures the application host builder with the specified configuration implementation.
    /// </summary>
    /// <param name="configuration">The configuration instance to be used by the application.</param>
    /// <returns>The current instance of the application host builder.</returns>
    public static BuilderConfigurationOptions WithJsonConfiguration(
        this BuilderConfigurationOptions options,
        string fileName,
        bool createIfNotExist,
        TimeSpan? flushToFileDelayMs
    )
    {
        options.CreateConfigCallback = () =>
            new JsonOneFileConfiguration(fileName, createIfNotExist, flushToFileDelayMs);
        return options;
    }
}

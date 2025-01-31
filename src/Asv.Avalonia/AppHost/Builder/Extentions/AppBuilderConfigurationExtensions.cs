using Asv.Cfg;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class AppBuilderConfigurationExtensions
{
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

    public static IAppHostBuilder UseConfig(
        this IAppHostBuilder builder,
        IConfiguration configuration
    )
    {
        builder.Core.Configuration = configuration;
        return builder;
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public static class AppBuilderLoggerExtensions
{
    public static IAppHostBuilder UseLogging(
        this IAppHostBuilder builder,
        BuilderLoggerOptions options
    )
    {
        var service = LogServiceBuilder.BuildFromOptions(builder.Core.Configuration, options);
        builder.Core.LogService = service;
        return builder;
    }

    public static IAppHostBuilder UseLogging(
        this IAppHostBuilder builder,
        Action<BuilderLoggerOptions>? setupAction = null
    )
    {
        var options = new BuilderLoggerOptions();

        setupAction?.Invoke(options);

        return builder.UseLogging(options);
    }
}

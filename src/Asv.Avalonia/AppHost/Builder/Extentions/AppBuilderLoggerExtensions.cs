using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public static class AppBuilderLoggerExtensions
{
    public static IAppHostBuilder UseLogging(
        this IAppHostBuilder builder,
        BuilderLoggerOptions options
    )
    {
        var service = LogServiceBuilder.BuildFromOptions(builder.Configuration, options);
        builder.Services.WithExport<ILogService>(service);
        builder.Services.WithExport<ILoggerFactory>(service);
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

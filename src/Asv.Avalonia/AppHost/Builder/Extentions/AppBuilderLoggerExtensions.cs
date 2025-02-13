namespace Asv.Avalonia;

public static class AppBuilderLoggerExtensions
{
    /// <summary>
    /// Adds LoggerService to the app core to enable Logging.
    /// </summary>
    /// <param name="builder">The AppHostBuilder to add the service to.</param>
    /// <param name="options">Options to set up the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public static IAppHostBuilder UseLogging(
        this IAppHostBuilder builder,
        BuilderLoggerOptions options
    )
    {
        ArgumentNullException.ThrowIfNull(builder.Core.Configuration);
        var service = LogServiceBuilder.BuildFromOptions(builder.Core.Configuration, options);
        builder.Core.LogService = service;
        return builder;
    }

    /// <summary>
    /// Adds LoggerService to the app core to enable Logging.
    /// </summary>
    /// <param name="builder">The AppHostBuilder to add the service to.</param>
    /// <param name="setupAction">Action to set up the service.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
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

namespace Asv.Avalonia;

public static class AppBuilderLoggerExtensions
{
    public static IAppHostBuilder UseLogging(
        this IAppHostBuilder builder,
        BuilderLoggerOptions options
    )
    {
        AppHostBuilder.Options.Add(typeof(BuilderLoggerOptions), options);
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

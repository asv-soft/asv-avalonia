namespace Asv.Avalonia;

public static class AppBuilderSingleInstanceExtensions
{
    public static IAppHostBuilder EnforceSingleInstance(
        this IAppHostBuilder builder,
        BuilderSingleInstanceOptions options
    )
    {
        builder.Core.MutexName = options.MutexName;
        builder.Core.NamedPipe = options.NamedPipe;
        return builder;
    }

    public static IAppHostBuilder EnforceSingleInstance(
        this IAppHostBuilder builder,
        Action<BuilderSingleInstanceOptions>? setupAction = null
    )
    {
        var options = new BuilderSingleInstanceOptions();

        setupAction?.Invoke(options);

        return builder.EnforceSingleInstance(options);
    }
}

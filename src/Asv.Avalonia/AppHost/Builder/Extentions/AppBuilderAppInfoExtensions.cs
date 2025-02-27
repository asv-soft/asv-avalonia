using System.Reflection;

namespace Asv.Avalonia;

public static class AppBuilderAppInfoExtensions
{
    /// <summary>
    /// Configures the application host builder with application information extracted from the specified assembly.
    /// </summary>
    /// <param name="builder">The AppHostBuilder to add the info from the assembly to.</param>
    /// <param name="assembly">The assembly from which to extract the application information such as version, product name, or company name.</param>
    /// <returns>The current instance of the application host builder.</returns>
    public static IAppHostBuilder SetAppInfoFrom(this IAppHostBuilder builder, Assembly assembly)
    {
        var options = new BuilderAppInfoOptions();

        options.WithProductNameFrom(assembly);
        options.WithVersionFrom(assembly);
        options.WithCompanyNameFrom(assembly);
        options.WithProductTitleFrom(assembly);
        options.WithAvaloniaVersionFrom(assembly);

        return builder.SetAppInfo(options);
    }

    public static IAppHostBuilder SetAppInfo(
        this IAppHostBuilder builder,
        BuilderAppInfoOptions options
    )
    {
        builder.Core.AppInfo.Name = options.Name;
        builder.Core.AppInfo.Version = options.Version;
        builder.Core.AppInfo.CompanyName = options.CompanyName;
        builder.Core.AppInfo.Title = options.ProductTitle;
        builder.Core.AppInfo.AvaloniaVersion = options.AvaloniaVersion;
        return builder;
    }

    public static IAppHostBuilder SetAppInfo(
        this IAppHostBuilder builder,
        Action<BuilderAppInfoOptions> setupAction
    )
    {
        var options = new BuilderAppInfoOptions();

        setupAction.Invoke(options);

        return builder.SetAppInfo(options);
    }
}

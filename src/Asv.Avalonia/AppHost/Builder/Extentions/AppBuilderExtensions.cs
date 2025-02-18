using Asv.Cfg;

namespace Asv.Avalonia;

public static class AppBuilderExtensions
{
    /// <summary>
    /// Sets the command-line arguments to be used by the application host.
    /// </summary>
    /// <param name="builder">The AppHostBuilder to add the args to.</param>
    /// <param name="args">An array of command-line arguments.</param>
    /// <returns>The current instance of the application host builder.</returns>
    public static IAppHostBuilder SetArguments(this IAppHostBuilder builder, string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        builder.Core.Args = new AppArgs(args);
        return builder;
    }

    /// <summary>
    /// Sets the user data folder path for the application.
    /// </summary>
    /// <param name="builder">The AppHostBuilder to add the userFolder to.</param>
    /// <param name="userFolder">The path to the folder where user data will be stored.</param>
    /// <returns>The current instance of the application host builder.</returns>
    public static IAppHostBuilder SetUserDataFolder(this IAppHostBuilder builder, string userFolder)
    {
        ArgumentNullException.ThrowIfNull(userFolder);

        builder.Core.UserDataFolder = (_, _) => userFolder;
        return builder;
    }

    /// <summary>
    /// Sets the user data folder path for the application.
    /// </summary>
    /// <param name="builder">The AppHostBuilder to add the userFolder to.</param>
    /// <param name="fromInfo">The userFolder callback.</param>
    /// <returns>The current instance of the application host builder.</returns>
    public static IAppHostBuilder SetUserDataFolder(
        this IAppHostBuilder builder,
        Func<IConfiguration, IAppInfo, string> fromInfo
    )
    {
        ArgumentNullException.ThrowIfNull(fromInfo);

        builder.Core.UserDataFolder = fromInfo;
        return builder;
    }
}

using System.Reflection;
using Asv.Cfg;
using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

/// <summary>
/// Defines methods for configuring and building an application host,
/// including settings for configuration, logging, application details,
/// and runtime properties.
/// </summary>
public interface IAppHostBuilder
{
    /// <summary>
    /// Configures the application host builder with the specified configuration implementation.
    /// </summary>
    /// <param name="configuration">The configuration instance to be used by the application.</param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithConfiguration(IConfiguration configuration);
    IAppHostBuilder WithJsonConfiguration(
        string fileName,
        bool createIfNotExist,
        TimeSpan? flushToFileDelayMs
    );

    /// <summary>
    /// Configures the application host builder with application information extracted from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly from which to extract the application information such as version, product name, or company name.</param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithAppInfoFrom(Assembly assembly);

    /// <summary>
    /// Configures the application host builder with the specified product name.
    /// </summary>
    /// <param name="appName">The product name to be used by the application.</param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithProductName(string appName);

    /// <summary>
    /// Configures the application host builder with the specified product name.
    /// </summary>
    /// <param name="assembly">The assembly from which to extract the product name <see cref="AssemblyProductAttribute"/>.</param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithProductName(Assembly assembly);

    /// <summary>
    /// Configures the application host builder with the specified product title.
    /// </summary>
    /// <param name="productTitle">The title of the product to be set for the application host.</param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithProductTitle(string productTitle);

    /// <summary>
    /// Configures the application host builder with the specified product title.
    /// </summary>
    /// <param name="assembly">The assembly from which to extract the product title <see cref="AssemblyTitleAttribute"/>.</param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithProductTitle(Assembly assembly);

    /// <summary>
    /// Configures the application host builder with the specified application version.
    /// </summary>
    /// <param name="version">The version of the application to be set in the host configuration.</param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithVersion(string version);

    /// <summary>
    /// Configures the application host builder with the version information
    /// retrieved from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly from which the version information will be retrieved <see cref="AssemblyVersionAttribute"/>.</param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithVersion(Assembly assembly);

    /// <summary>
    /// Sets the company name for the application host builder.
    /// </summary>
    /// <param name="companyName">The name of the company to be associated with the application.</param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithCompanyName(string companyName);

    /// <summary>
    /// Configures the application host builder with the company name obtained from the specified assembly's metadata.
    /// </summary>
    /// <param name="assembly">The assembly from which to extract the company name <see cref="AssemblyCompanyAttribute"/>.</param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithCompanyName(Assembly assembly);

    /// <summary>
    /// Configures the application host builder with the specified Avalonia version.
    /// </summary>
    /// <param name="avaloniaVersion">The version of Avalonia to be used by the application.</param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithAvaloniaVersion(string avaloniaVersion);

    /// <summary>
    /// Configures the application host builder with the specified minimum log level.
    /// </summary>
    /// <param name="logLevel">The minimum log level to be used for logging.</param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithLogMinimumLevel(LogLevel logLevel);

    /// <summary>
    /// Configures the application host builder with the specified minimum logging level.
    /// </summary>
    /// <param name="fromConfig">A function that extracts the minimum log level from a configuration object.</param>
    /// <typeparam name="TConfig">The type of the configuration object. It will be loaded from the <see cref="IConfiguration"/>.</typeparam>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithLogMinimumLevel<TConfig>(Func<TConfig, LogLevel> fromConfig)
        where TConfig : new();

    /// <summary>
    /// Configures the application host builder to use a JSON log file stored in the specified folder with a rolling file size limit.
    /// </summary>
    /// <typeparam name="TConfig">The type used for retrieving configuration values.</typeparam>
    /// <param name="logFolder">A function that provides the folder path for log files based on the configuration.</param>
    /// <param name="rollingSizeKb">A function that specifies the file size limit, in kilobytes, for rolling log files based on the configuration.</param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithJsonLogFolder<TConfig>(
        Func<TConfig, string> logFolder,
        Func<TConfig, int> rollingSizeKb
    )
        where TConfig : new();

    /// <summary>
    /// Sets the folder for JSON log storage and configures the rolling log file size in kilobytes.
    /// </summary>
    /// <param name="logFolder">The path to the folder where log files will be stored.</param>
    /// <param name="rollingSizeKb">A function that retrieves the rolling file size in kilobytes from the configuration.</param>
    /// <typeparam name="TConfig">The type of the configuration class.</typeparam>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithJsonLogFolder<TConfig>(string logFolder, Func<TConfig, int> rollingSizeKb)
        where TConfig : new();

    /// <summary>
    /// Configures the application host builder to use a JSON log folder with the specified directory path and file size rolling threshold.
    /// </summary>
    /// <param name="logFolder">The directory path where the JSON log files will be stored.</param>
    /// <param name="rollingSizeKb">The maximum size of a log file in kilobytes before it rolls over to a new file.</param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithJsonLogFolder(string logFolder, int rollingSizeKb);

    /// <summary>
    /// Enables or disables logging to the console for the application.
    /// </summary>
    /// <param name="enabled">A boolean value indicating whether logging to the console is enabled. Defaults to true.</param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithLogToConsole(bool enabled = true);

    /// <summary>
    /// Sets the command-line arguments to be used by the application host.
    /// </summary>
    /// <param name="args">An array of command-line arguments.</param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithArguments(string[] args);

    /// <summary>
    /// Sets the user data folder path for the application.
    /// </summary>
    /// <param name="userFolder">The path to the folder where user data will be stored.</param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder WithUserDataFolder(string userFolder);

    /// <summary>
    /// Ensures that only a single instance of the application is running on the system.
    /// If another instance is already running, the current instance will not proceed further.
    /// </summary>
    /// <param name="mutexName"> Unique mutex name. </param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder EnforceSingleInstance(string? mutexName = null);

    /// <summary>
    /// Enables forwarding of command-line arguments to an already running instance of the application.
    /// If the application is not already running, the current instance will handle the arguments as usual.
    /// Must be called with <see cref="EnforceSingleInstance"/>.
    /// </summary>
    /// <param name="namedPipeName"> Unique pipe name. </param>
    /// <returns>The current instance of the application host builder.</returns>
    IAppHostBuilder EnableArgumentForwarding(string? namedPipeName = null);

    /// <summary>
    /// Configures the application to use a plugin manager that references the specified API package.
    /// </summary>
    /// <param name="apiPackageName">The NuGet package name or identifier for the plugin API.</param>
    /// <param name="nugetPluginName">The string all dependent plugins start with.</param>
    /// <param name="apiVersion">The plugin API package version.</param>
    /// <returns>The current instance of the <see cref="IAppHostBuilder"/>.</returns>
    IAppHostBuilder WithPluginsManager(
        string apiPackageName,
        string nugetPluginName,
        SemVersion apiVersion
    );
}

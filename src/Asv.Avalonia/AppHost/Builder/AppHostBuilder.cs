using System.Reflection;
using Asv.Cfg;
using Avalonia;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

internal class AppHostBuilder : IAppHostBuilder
{
    private const string ZeroVersion = "0.0.0";
    private Func<IConfiguration> _createConfigCallback;
    private string _appName = string.Empty;
    private string _appVersion = ZeroVersion;
    private string _companyName = string.Empty;
    private string _avaloniaVersion = ZeroVersion;
    private AppArgs _args = new([]);
    private Func<IConfiguration, IAppInfo, string> _userDataFolder;
    private string _productTitle = string.Empty;
    private readonly string _appFolder;
    private Func<IAppInfo, string?> _mutexName;
    private Func<IAppInfo, string?> _namedPipe;

    private Func<IConfiguration, LogLevel> _setMinLevelCallback;
    private Func<IConfiguration, string> _logFolder;
    private Func<IConfiguration, int> _rollingSizeKb;
    private bool _logToConsole;

    public AppHostBuilder()
    {
        _userDataFolder = (_, info) =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                info.Name
            );
        _appFolder =
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        _createConfigCallback = () => new JsonOneFileConfiguration("config.json", true, null);
        _setMinLevelCallback = _ => LogLevel.Information;
        _mutexName = _ => null;
        _namedPipe = _ => null;
        _logFolder = _ => Path.Combine(_appFolder, "logs");
        _rollingSizeKb = _ => 1024 * 10;

        WithAppInfoFrom(Assembly.GetExecutingAssembly());
        WithAvaloniaVersion(
            typeof(AppBuilder).Assembly.GetName().Version?.ToString() ?? ZeroVersion
        );
        WithLogMinimumLevel(LogLevel.Information);
    }

    internal IAppHost Create()
    {
        var config = _createConfigCallback();
        var appInfo = new AppInfo
        {
            Name = _appName,
            Version = _appVersion,
            CompanyName = _companyName,
            AvaloniaVersion = _avaloniaVersion,
            Title = _productTitle,
        };
        var appPath = new AppPath
        {
            UserDataFolder = _userDataFolder(config, appInfo),
            AppFolder = _appFolder,
        };

        var minLevel = _setMinLevelCallback(config);
        var logFolder = _logFolder(config);
        var rollingSize = _rollingSizeKb(config);
        var logService = new LogService(logFolder, rollingSize, minLevel, _logToConsole);

        return new AppHost(
            config,
            appPath,
            appInfo,
            logService,
            _args,
            _mutexName(appInfo),
            _namedPipe(appInfo)
        );
    }

    #region SingleInstance

    public IAppHostBuilder EnforceSingleInstance(string? mutexName = null)
    {
        _mutexName = info => mutexName ?? info.Name;
        return this;
    }

    public IAppHostBuilder EnableArgumentForwarding(string? namedPipeName = null)
    {
        _namedPipe = info => namedPipeName ?? info.Name;
        return this;
    }

    #endregion

    #region Logging

    public IAppHostBuilder WithLogMinimumLevel(LogLevel minLogLevel)
    {
        _setMinLevelCallback = _ => minLogLevel;
        return this;
    }

    public IAppHostBuilder WithLogMinimumLevel<TConfig>(Func<TConfig, LogLevel> fromConfig)
        where TConfig : new()
    {
        _setMinLevelCallback = x => fromConfig(x.Get<TConfig>());
        return this;
    }

    public IAppHostBuilder WithJsonLogFolder<TConfig>(
        Func<TConfig, string> logFolder,
        Func<TConfig, int> rollingSizeKb
    )
        where TConfig : new()
    {
        _logFolder = x => logFolder(x.Get<TConfig>());
        _rollingSizeKb = x => rollingSizeKb(x.Get<TConfig>());
        return this;
    }

    public IAppHostBuilder WithJsonLogFolder<TConfig>(
        string logFolder,
        Func<TConfig, int> rollingSizeKb
    )
        where TConfig : new()
    {
        _logFolder = _ => logFolder;
        _rollingSizeKb = x => rollingSizeKb(x.Get<TConfig>());
        return this;
    }

    public IAppHostBuilder WithJsonLogFolder(string logFolder, int rollingSizeKb)
    {
        _logFolder = _ => logFolder;
        _rollingSizeKb = _ => rollingSizeKb;
        return this;
    }

    public IAppHostBuilder WithLogToConsole(bool enabled = true)
    {
        _logToConsole = enabled;
        return this;
    }

    public IAppHostBuilder WithArguments(string[] args)
    {
        _args = new AppArgs(args);
        return this;
    }

    public IAppHostBuilder WithUserDataFolder(string userFolder)
    {
        _userDataFolder = (_, _) => userFolder;
        return this;
    }

    #endregion

    #region Configuration

    public IAppHostBuilder WithConfiguration(IConfiguration configuration)
    {
        _createConfigCallback = () => configuration;
        return this;
    }

    public IAppHostBuilder WithJsonConfiguration(
        string fileName,
        bool createIfNotExist,
        TimeSpan? flushToFileDelayMs
    )
    {
        _createConfigCallback = () =>
            new JsonOneFileConfiguration(fileName, createIfNotExist, flushToFileDelayMs);
        return this;
    }

    public IAppHostBuilder WithAppInfoFrom(Assembly assembly)
    {
        WithProductName(assembly);
        WithVersion(assembly);
        WithCompanyName(assembly);
        WithProductTitle(assembly);
        return this;
    }

    #endregion

    #region ProductTitle

    public IAppHostBuilder WithProductTitle(string productTitle)
    {
        _productTitle = productTitle;
        return this;
    }

    public IAppHostBuilder WithProductTitle(Assembly assembly)
    {
        var attributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
        if (attributes.Length > 0)
        {
            var titleAttribute = (AssemblyTitleAttribute)attributes[0];
            if (titleAttribute.Title.Length > 0)
            {
                _productTitle = titleAttribute.Title;
            }
        }
        else
        {
            _productTitle = assembly.GetName().Name ?? string.Empty;
        }

        return this;
    }
    #endregion

    #region AvaloniaVersion

    public IAppHostBuilder WithAvaloniaVersion(string avaloniaVersion)
    {
        _avaloniaVersion = avaloniaVersion;
        return this;
    }

    #endregion

    #region AppName

    public IAppHostBuilder WithProductName(string appName)
    {
        _appName = appName;
        return this;
    }

    public IAppHostBuilder WithProductName(Assembly assembly)
    {
        var attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
        if (attributes.Length == 0)
        {
            _appName = assembly.GetName().Name ?? string.Empty;
        }
        else
        {
            _appName = ((AssemblyProductAttribute)attributes[0]).Product;
        }

        return this;
    }

    #endregion

    #region Version

    public IAppHostBuilder WithVersion(string version)
    {
        _appVersion = version;
        return this;
    }

    public IAppHostBuilder WithVersion(Assembly assembly)
    {
        var attributes = assembly.GetCustomAttributes(
            typeof(AssemblyInformationalVersionAttribute),
            false
        );

        _appVersion =
            attributes.Length == 0
                ? ZeroVersion
                : ((AssemblyInformationalVersionAttribute)attributes[0]).InformationalVersion;
        return this;
    }

    #endregion

    #region CompanyName

    public IAppHostBuilder WithCompanyName(string companyName)
    {
        _companyName = companyName;
        return this;
    }

    public IAppHostBuilder WithCompanyName(Assembly assembly)
    {
        var attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
        _companyName =
            attributes.Length == 0
                ? string.Empty
                : ((AssemblyCompanyAttribute)attributes[0]).Company;
        return this;
    }

    #endregion
}

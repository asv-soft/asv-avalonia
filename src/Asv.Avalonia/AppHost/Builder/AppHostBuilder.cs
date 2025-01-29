using System.Composition.Hosting;
using System.Reflection;
using Asv.Cfg;
using Avalonia;
using Type = System.Type;

namespace Asv.Avalonia;

internal class AppHostBuilder : IAppHostBuilder
{
    public IDictionary<Type, IBuilderOptions> Options { get; }
    public IConfiguration Configuration { get; private set; }
    public ContainerConfiguration Services { get; }
    public ILogService LogService { get; set; }

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

    public AppHostBuilder()
    {
        Options = new Dictionary<Type, IBuilderOptions>();
        Services = new ContainerConfiguration();
        _userDataFolder = (_, info) =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                info.Name
            );
        _appFolder =
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        _createConfigCallback = () => new JsonOneFileConfiguration("config.json", true, null);
        Configuration = _createConfigCallback();
        _mutexName = _ => null;
        _namedPipe = _ => null;

        WithAppInfoFrom(Assembly.GetExecutingAssembly());
        WithAvaloniaVersion(
            typeof(AppBuilder).Assembly.GetName().Version?.ToString() ?? ZeroVersion
        );
    }

    internal IAppHost Create()
    {
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
            UserDataFolder = _userDataFolder(Configuration, appInfo),
            AppFolder = _appFolder,
        };

        return new AppHost(
            Configuration,
            appPath,
            appInfo,
            LogService,
            _args,
            Services,
            _mutexName(appInfo),
            _namedPipe(appInfo)
        );
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

    #region Configuration

    public IAppHostBuilder WithConfiguration(IConfiguration configuration)
    {
        _createConfigCallback = () => configuration;
        Configuration = _createConfigCallback();
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
        Configuration = _createConfigCallback();
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

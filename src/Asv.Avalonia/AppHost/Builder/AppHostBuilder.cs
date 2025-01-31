using System.Composition.Hosting;
using System.Reflection;
using Asv.Cfg;
using Avalonia;

namespace Asv.Avalonia;

internal class AppHostBuilder : IAppHostBuilder
{
    public IAppCore Core { get; }

    public AppHostBuilder()
    {
        Core = new AppCore
        {
            UserDataFolder = (_, info) =>
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    info.Name
                ),
            AppFolder =
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
            Configuration = new JsonOneFileConfiguration("config.json", true, null),
            Services = new ContainerConfiguration(),
        };

        WithAppInfoFrom(Assembly.GetExecutingAssembly());
        WithAvaloniaVersion(
            typeof(AppBuilder).Assembly.GetName().Version?.ToString() ?? Core.AppVersion
        );
    }

    internal IAppHost Create()
    {
        return new AppHost(Core);
    }

    public IAppHostBuilder WithArguments(string[] args)
    {
        Core.Args = new AppArgs(args);
        return this;
    }

    public IAppHostBuilder WithUserDataFolder(string userFolder)
    {
        Core.UserDataFolder = (_, _) => userFolder;
        return this;
    }

    #region SingleInstance

    public IAppHostBuilder EnforceSingleInstance(string? mutexName = null)
    {
        Core.MutexName = info => mutexName ?? info.Name;
        return this;
    }

    public IAppHostBuilder EnableArgumentForwarding(string? namedPipeName = null)
    {
        Core.NamedPipe = info => namedPipeName ?? info.Name;
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
        Core.ProductTitle = productTitle;
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
                Core.ProductTitle = titleAttribute.Title;
            }
        }
        else
        {
            Core.ProductTitle = assembly.GetName().Name ?? string.Empty;
        }

        return this;
    }
    #endregion

    #region AvaloniaVersion

    public IAppHostBuilder WithAvaloniaVersion(string avaloniaVersion)
    {
        Core.AvaloniaVersion = avaloniaVersion;
        return this;
    }

    #endregion

    #region AppName

    public IAppHostBuilder WithProductName(string appName)
    {
        Core.AppName = appName;
        return this;
    }

    public IAppHostBuilder WithProductName(Assembly assembly)
    {
        var attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
        if (attributes.Length == 0)
        {
            Core.AppName = assembly.GetName().Name ?? string.Empty;
        }
        else
        {
            Core.AppName = ((AssemblyProductAttribute)attributes[0]).Product;
        }

        return this;
    }

    #endregion

    #region Version

    public IAppHostBuilder WithVersion(string version)
    {
        Core.AppVersion = version;
        return this;
    }

    public IAppHostBuilder WithVersion(Assembly assembly)
    {
        var attributes = assembly.GetCustomAttributes(
            typeof(AssemblyInformationalVersionAttribute),
            false
        );

        if (attributes.Length == 0)
        {
            return this;
        }

        Core.AppVersion = (
            (AssemblyInformationalVersionAttribute)attributes[0]
        ).InformationalVersion;
        return this;
    }

    #endregion

    #region CompanyName

    public IAppHostBuilder WithCompanyName(string companyName)
    {
        Core.CompanyName = companyName;
        return this;
    }

    public IAppHostBuilder WithCompanyName(Assembly assembly)
    {
        var attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
        Core.CompanyName =
            attributes.Length == 0
                ? string.Empty
                : ((AssemblyCompanyAttribute)attributes[0]).Company;
        return this;
    }

    #endregion
}

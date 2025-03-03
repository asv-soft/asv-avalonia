using System.Reflection;
using Avalonia;

namespace Asv.Avalonia;

public class AppInfoBuilder
{
    private readonly AppInfo _options = new();

    internal AppInfoBuilder()
    {
        FillFromAssembly(typeof(AppBuilder).Assembly);
    }

    /// <summary>
    /// Configures the application host builder with the specified product name.
    /// </summary>
    /// <param name="appName">The product name to be used by the application.</param>
    /// <returns> The application host builder.</returns>
    public AppInfoBuilder WithProductName(string appName)
    {
        ArgumentNullException.ThrowIfNull(appName);
        _options.Name = appName;
        return this;
    }

    /// <summary>
    /// Configures the application host builder with the specified product name.
    /// </summary>
    /// <param name="assembly">The assembly from which to extract the product name <see cref="AssemblyProductAttribute"/>.</param>
    /// <returns>The current instance of the options.</returns>
    public AppInfoBuilder WithProductNameFrom(Assembly assembly)
    {
        var attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);

        ArgumentNullException.ThrowIfNull(attributes);
        if (attributes.Length != 0)
        {
            var nameAttribute = (AssemblyProductAttribute)attributes[0];
            if (nameAttribute.Product.Length != 0)
            {
                _options.Name = nameAttribute.Product;
                return this;
            }
        }

        var name = assembly.GetName().Name;
        ArgumentNullException.ThrowIfNull(name);
        _options.Name = name;
        return this;
    }

    /// <summary>
    /// Configures the application host builder with the specified application version.
    /// </summary>
    /// <param name="version">The version of the application to be set in the host configuration.</param>
    /// <returns>The current instance of the options.</returns>
    public AppInfoBuilder WithVersion(string version)
    {
        ArgumentNullException.ThrowIfNull(version);
        _options.Version = version;
        return this;
    }

    /// <summary>
    /// Configures the application host builder with the version information
    /// retrieved from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly from which the version information will be retrieved <see cref="AssemblyVersionAttribute"/>.</param>
    /// <returns>The current instance of the options.</returns>
    public AppInfoBuilder WithVersionFrom(Assembly assembly)
    {
        var attributes = assembly.GetCustomAttributes(
            typeof(AssemblyInformationalVersionAttribute),
            false
        );

        ArgumentNullException.ThrowIfNull(attributes);
        if (attributes.Length == 0)
        {
            throw new ArgumentNullException(nameof(attributes));
        }

        var nameAttribute = (AssemblyInformationalVersionAttribute)attributes[0];
        ArgumentException.ThrowIfNullOrEmpty(nameAttribute.InformationalVersion);

        _options.Version = nameAttribute.InformationalVersion;
        return this;
    }

    /// <summary>
    /// Sets the company name for the application host builder.
    /// </summary>
    /// <param name="companyName">The name of the company to be associated with the application.</param>
    /// <returns>The current instance of the options.</returns>
    public AppInfoBuilder WithCompanyName(string companyName)
    {
        ArgumentNullException.ThrowIfNull(companyName);

        _options.CompanyName = companyName;
        return this;
    }

    /// <summary>
    /// Configures the application host builder with the company name obtained from the specified assembly's metadata.
    /// </summary>
    /// <param name="assembly">The assembly from which to extract the company name <see cref="AssemblyCompanyAttribute"/>.</param>
    /// <returns>The current instance of the options.</returns>
    public AppInfoBuilder WithCompanyNameFrom(Assembly assembly)
    {
        var attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);

        ArgumentNullException.ThrowIfNull(attributes);
        if (attributes.Length == 0)
        {
            throw new ArgumentNullException(nameof(attributes));
        }

        var nameAttribute = (AssemblyCompanyAttribute)attributes[0];
        ArgumentException.ThrowIfNullOrEmpty(nameAttribute.Company);

        _options.CompanyName = nameAttribute.Company;
        return this;
    }

    /// <summary>
    /// Configures the application host builder with the specified Avalonia version.
    /// </summary>
    /// <param name="avaloniaVersion">The version of Avalonia to be used by the application.</param>
    /// /// <returns>The current instance of the options.</returns>
    public AppInfoBuilder WithAvaloniaVersion(string avaloniaVersion)
    {
        ArgumentNullException.ThrowIfNull(avaloniaVersion);
        _options.AvaloniaVersion = avaloniaVersion;
        return this;
    }

    /// <summary>
    /// Configures the application host builder with the specified Avalonia version.
    /// </summary>
    /// <param name="assembly">The assembly from which to extract the avalonia version <see cref="AssemblyProductAttribute"/>.</param>
    /// /// <returns>The current instance of the options.</returns>
    public AppInfoBuilder WithAvaloniaVersionFrom(Assembly assembly)
    {
        var version = assembly.GetName().Version?.ToString();
        ArgumentException.ThrowIfNullOrEmpty(version);
        _options.AvaloniaVersion = version;
        return this;
    }

    /// <summary>
    /// Configures the application host builder with the specified product title.
    /// </summary>
    /// <param name="productTitle">The title of the product to be set for the application host.</param>
    /// <returns>The current instance of the options.</returns>
    public AppInfoBuilder WithProductTitle(string productTitle)
    {
        ArgumentNullException.ThrowIfNull(productTitle);
        _options.Title = productTitle;
        return this;
    }

    /// <summary>
    /// Configures the application host builder with the specified product title.
    /// </summary>
    /// <param name="assembly">The assembly from which to extract the product title <see cref="AssemblyTitleAttribute"/>.</param>
    /// <returns>The current instance of the options.</returns>
    public AppInfoBuilder WithProductTitleFrom(Assembly assembly)
    {
        var attributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);

        ArgumentNullException.ThrowIfNull(attributes);
        if (attributes.Length != 0)
        {
            var titleAttribute = (AssemblyTitleAttribute)attributes[0];
            if (titleAttribute.Title.Length != 0)
            {
                _options.Title = titleAttribute.Title;
                return this;
            }
        }

        var name = assembly.GetName().Name;
        ArgumentNullException.ThrowIfNull(name);
        _options.Title = name;
        return this;
    }

    public IAppInfo Build()
    {
        return _options;
    }

    public void FillFromAssembly(Assembly assembly)
    {
        WithProductNameFrom(assembly);
        WithVersionFrom(assembly);
        WithCompanyNameFrom(assembly);
        WithProductTitleFrom(assembly);
        WithAvaloniaVersionFrom(typeof(AppBuilder).Assembly);
    }
}

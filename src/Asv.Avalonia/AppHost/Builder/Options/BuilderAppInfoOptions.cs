using System.Reflection;

namespace Asv.Avalonia;

public sealed class BuilderAppInfoOptions
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string ProductTitle { get; set; } = string.Empty;
    public string AvaloniaVersion { get; set; } = string.Empty;
}

public static class BuilderAppInfoOptionsExtensions
{
    /// <summary>
    /// Configures the application host builder with the specified product name.
    /// </summary>
    /// <param name="options">The app info options to add the appName to.</param>
    /// <param name="appName">The product name to be used by the application.</param>
    public static void WithProductName(this BuilderAppInfoOptions options, string appName)
    {
        ArgumentNullException.ThrowIfNull(appName);
        options.Name = appName;
    }

    /// <summary>
    /// Configures the application host builder with the specified product name.
    /// </summary>
    /// <param name="options">The app info options to add the info from the assembly to.</param>
    /// <param name="assembly">The assembly from which to extract the product name <see cref="AssemblyProductAttribute"/>.</param>
    public static void WithProductNameFrom(this BuilderAppInfoOptions options, Assembly assembly)
    {
        var attributes = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);

        ArgumentNullException.ThrowIfNull(attributes);
        if (attributes.Length != 0)
        {
            var nameAttribute = (AssemblyProductAttribute)attributes[0];
            if (nameAttribute.Product.Length != 0)
            {
                options.Name = nameAttribute.Product;
                return;
            }
        }

        var name = assembly.GetName().Name;
        ArgumentNullException.ThrowIfNull(name);
        options.Name = name;
    }

    /// <summary>
    /// Configures the application host builder with the specified application version.
    /// </summary>
    /// <param name="options">The app info options to add the version to.</param>
    /// <param name="version">The version of the application to be set in the host configuration.</param>
    public static void WithVersion(this BuilderAppInfoOptions options, string version)
    {
        ArgumentNullException.ThrowIfNull(version);
        options.Version = version;
    }

    /// <summary>
    /// Configures the application host builder with the version information
    /// retrieved from the specified assembly.
    /// </summary>
    /// <param name="options">The app info options to add the info from the assembly to.</param>
    /// <param name="assembly">The assembly from which the version information will be retrieved <see cref="AssemblyVersionAttribute"/>.</param>
    public static void WithVersionFrom(this BuilderAppInfoOptions options, Assembly assembly)
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

        options.Version = nameAttribute.InformationalVersion;
    }

    /// <summary>
    /// Sets the company name for the application host builder.
    /// </summary>
    /// <param name="options">The app info options to add the companyName to.</param>
    /// <param name="companyName">The name of the company to be associated with the application.</param>
    /// <returns>The current instance of the options.</returns>
    public static void WithCompanyName(this BuilderAppInfoOptions options, string companyName)
    {
        ArgumentNullException.ThrowIfNull(companyName);

        options.CompanyName = companyName;
    }

    /// <summary>
    /// Configures the application host builder with the company name obtained from the specified assembly's metadata.
    /// </summary>
    /// <param name="options">The app info options to add the info from the assembly to.</param>
    /// <param name="assembly">The assembly from which to extract the company name <see cref="AssemblyCompanyAttribute"/>.</param>
    public static void WithCompanyNameFrom(this BuilderAppInfoOptions options, Assembly assembly)
    {
        var attributes = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);

        ArgumentNullException.ThrowIfNull(attributes);
        if (attributes.Length == 0)
        {
            throw new ArgumentNullException(nameof(attributes));
        }

        var nameAttribute = (AssemblyCompanyAttribute)attributes[0];
        ArgumentException.ThrowIfNullOrEmpty(nameAttribute.Company);

        options.CompanyName = nameAttribute.Company;
    }

    /// <summary>
    /// Configures the application host builder with the specified Avalonia version.
    /// </summary>
    /// <param name="options">The app info options to add the avaloniaVersion to.</param>
    /// <param name="avaloniaVersion">The version of Avalonia to be used by the application.</param>
    public static void WithAvaloniaVersion(
        this BuilderAppInfoOptions options,
        string avaloniaVersion
    )
    {
        ArgumentNullException.ThrowIfNull(avaloniaVersion);
        options.AvaloniaVersion = avaloniaVersion;
    }

    /// <summary>
    /// Configures the application host builder with the specified Avalonia version.
    /// </summary>
    /// <param name="options">The app info options to add the info from the assembly to.</param>
    /// <param name="assembly">The assembly from which to extract the avalonia version <see cref="AssemblyProductAttribute"/>.</param>
    public static void WithAvaloniaVersionFrom(
        this BuilderAppInfoOptions options,
        Assembly assembly
    )
    {
        var version = assembly.GetName().Version?.ToString();
        ArgumentException.ThrowIfNullOrEmpty(version);
        options.AvaloniaVersion = version;
    }

    /// <summary>
    /// Configures the application host builder with the specified product title.
    /// </summary>
    /// <param name="options">The AppHostBuilder to add the productTitle to.</param>
    /// <param name="productTitle">The title of the product to be set for the application host.</param>
    public static void WithProductTitle(this BuilderAppInfoOptions options, string productTitle)
    {
        ArgumentNullException.ThrowIfNull(productTitle);
        options.ProductTitle = productTitle;
    }

    /// <summary>
    /// Configures the application host builder with the specified product title.
    /// </summary>
    /// <param name="options">The AppHostBuilder to add the info from the assembly to.</param>
    /// <param name="assembly">The assembly from which to extract the product title <see cref="AssemblyTitleAttribute"/>.</param>
    public static void WithProductTitleFrom(this BuilderAppInfoOptions options, Assembly assembly)
    {
        var attributes = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);

        ArgumentNullException.ThrowIfNull(attributes);
        if (attributes.Length != 0)
        {
            var titleAttribute = (AssemblyTitleAttribute)attributes[0];
            if (titleAttribute.Title.Length != 0)
            {
                options.ProductTitle = titleAttribute.Title;
                return;
            }
        }

        var name = assembly.GetName().Name;
        ArgumentNullException.ThrowIfNull(name);
        options.ProductTitle = name;
    }
}

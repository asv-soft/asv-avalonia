namespace Asv.Avalonia;

public interface IAppInfo
{
    /// <summary>
    /// Gets application name.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Gets application name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets application version.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets authors.
    /// </summary>
    string CompanyName { get; }

    /// <summary>
    /// Gets avalonia UI package version.
    /// </summary>
    string AvaloniaVersion { get; }
}

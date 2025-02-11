namespace Asv.Avalonia;

/// <summary>
/// Defines methods for configuring and building an application host,
/// including settings for configuration, logging, application details,
/// and runtime properties.
/// </summary>
public interface IAppHostBuilder
{
    public IAppCore Core { get; }
    public IAppHost Build();
}

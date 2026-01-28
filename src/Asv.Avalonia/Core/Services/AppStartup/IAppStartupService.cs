namespace Asv.Avalonia;

/// <summary>
/// Defines a contract for services that must be loaded and executed
/// during the Avalonia <see cref="App"/> startup lifecycle.
/// </summary>
public interface IAppStartupService
{
    /// <summary>
    /// Called very early, inside the <see cref="App"/> constructor,
    /// before Avalonia framework initialization begins.
    /// Use this for preparing critical dependencies that must exist
    /// before the application is fully constructed.
    /// </summary>
    void AppCtor();

    /// <summary>
    /// Called after Avalonia has completed its framework initialization.
    /// Use this stage to configure services or logic that depend on
    /// Avalonia being initialized (e.g., themes, resources, platform-specific services).
    /// </summary>
    void OnFrameworkInitializationCompleted();

    /// <summary>
    /// Called during application startup after the framework is initialized.
    /// Use this for general application initialization such as registering
    /// services, starting background tasks, or performing actions required
    /// before the main window is shown.
    /// </summary>
    void Initialize();
}

namespace Asv.Avalonia;

public interface IStartupTask
{
    void AppCtor();
    void OnFrameworkInitializationCompleted();
    void Initialize();
}

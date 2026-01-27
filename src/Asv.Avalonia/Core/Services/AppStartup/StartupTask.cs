namespace Asv.Avalonia;

public abstract class StartupTask : IStartupTask
{
    public virtual void AppCtor()
    {
        // do nothing
    }

    public void OnFrameworkInitializationCompleted()
    {
        // do nothing
    }

    public void Initialize()
    {
        // do nothing
    }
}

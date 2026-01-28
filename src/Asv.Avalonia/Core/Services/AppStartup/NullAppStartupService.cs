using DotNext.Patterns;

namespace Asv.Avalonia;

public class NullAppStartupService : IAppStartupService
{
    public static IAppStartupService Instance { get; } = new NullAppStartupService();

    public void AppCtor()
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

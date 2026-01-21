namespace Asv.Avalonia;

public class NullContainerHost : IContainerHost
{
    public static IContainerHost Instance { get; } = new NullContainerHost();

    public T GetExport<T>(string contract)
        where T : IExportable
    {
        throw new NotImplementedException();
    }

    public T GetExport<T>()
        where T : IExportable
    {
        if (typeof(T) == typeof(ILayoutService))
        {
            return (T)NullLayoutService.Instance;
        }

        if (typeof(T) == typeof(INavigationService))
        {
            return (T)NullNavigationService.Instance;
        }

        if (typeof(T) == typeof(ICommandService))
        {
            return (T)NullCommandService.Instance;
        }

        if (typeof(T) == typeof(IDialogService))
        {
            return (T)NullDialogService.Instance;
        }

        return default!;
    }

    public bool TryGetExport<T>(string id, out T value)
        where T : IExportable
    {
        value = GetExport<T>();
        return false;
    }

    public void SatisfyImports(object objectWithLooseImports)
    {
        // Do nothing
    }

    public IExportInfo Source => SystemModule.Instance;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

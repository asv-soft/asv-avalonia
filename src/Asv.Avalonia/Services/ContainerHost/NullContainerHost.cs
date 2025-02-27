using System.Composition.Hosting;

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
        return default!;
    }

    public bool TryGetExport<T>(string id, out T value)
        where T : IExportable
    {
        value = default!;
        return false;
    }

    public IExportInfo Source => SystemModule.Instance;
}

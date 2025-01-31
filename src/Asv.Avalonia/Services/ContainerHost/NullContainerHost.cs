using System.Composition.Hosting;

namespace Asv.Avalonia;

public class NullContainerHost : IContainerHost
{
    public static IContainerHost Instance { get; } = new NullContainerHost();
    public T GetExport<T>()
    {
        return default!;
    }

    public bool TryGetExport<T>(string id, out T value)
    {
        value = default!;
        return false;
    }
}
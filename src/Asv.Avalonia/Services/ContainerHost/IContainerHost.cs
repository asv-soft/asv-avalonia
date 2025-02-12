using System.Composition.Hosting;

namespace Asv.Avalonia;

public interface IContainerHost
{
    T GetExport<T>(string contract);
    T GetExport<T>();
    bool TryGetExport<T>(string id, out T value);
}

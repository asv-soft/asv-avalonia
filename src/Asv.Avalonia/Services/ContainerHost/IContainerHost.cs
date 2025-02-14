using System.Composition.Hosting;

namespace Asv.Avalonia;

public interface IContainerHost : IExportable
{
    T GetExport<T>(string contract)
        where T : IExportable;
    T GetExport<T>()
        where T : IExportable;
    bool TryGetExport<T>(string id, out T value)
        where T : IExportable;
}

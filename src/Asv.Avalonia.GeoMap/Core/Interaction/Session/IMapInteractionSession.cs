using R3;

namespace Asv.Avalonia.GeoMap;

public interface IMapInteractionSession : IDisposable
{
    MapInteractionLifecycle Lifecycle { get; }

    CompositeDisposable Disposable { get; }
}

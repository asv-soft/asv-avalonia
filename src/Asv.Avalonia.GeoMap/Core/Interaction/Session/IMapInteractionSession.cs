using R3;

namespace Asv.Avalonia.GeoMap;

public interface IMapInteractionSession : IDisposable
{
    CompositeDisposable Disposable { get; }
}

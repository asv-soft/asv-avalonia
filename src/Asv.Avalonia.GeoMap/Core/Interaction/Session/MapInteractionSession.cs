using R3;

namespace Asv.Avalonia.GeoMap;

internal sealed class MapInteractionSession : IMapInteractionSession
{
    private readonly IMapInteractionSessionOwner _owner;

    internal MapInteractionSession(IMapInteractionSessionOwner owner)
    {
        _owner = owner;
        Disposable = new CompositeDisposable();
    }

    public void Dispose()
    {
        _owner.End(this);
        Disposable.Dispose();
    }

    public CompositeDisposable Disposable { get; }
}

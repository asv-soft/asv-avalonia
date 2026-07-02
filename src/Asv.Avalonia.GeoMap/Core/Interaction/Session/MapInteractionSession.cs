using Asv.Common;
using R3;

namespace Asv.Avalonia.GeoMap;

/// <inheritdoc cref="IMapInteractionSession" />
internal sealed class MapInteractionSession : DisposableOnce, IMapInteractionSession
{
    private readonly IMapInteractionSessionOwner _owner;

    internal MapInteractionSession(
        IMapInteractionSessionOwner owner,
        MapInteractionLifecycle lifecycle
    )
    {
        _owner = owner;
        Lifecycle = lifecycle;
        Disposable = new CompositeDisposable();
    }

    protected override void InternalDisposeOnce()
    {
        _owner.End(this);
        Disposable.Dispose();
    }

    public MapInteractionLifecycle Lifecycle { get; }

    public CompositeDisposable Disposable { get; }
}

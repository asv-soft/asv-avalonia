using Asv.Common;
using R3;

namespace Asv.Avalonia.GeoMap;

public sealed class MapInteractionSession : IMapInteractionSession
{
    private readonly MapInteractionController _owner;
    private readonly Subject<GeoPoint> _clicked = new();
    private readonly Subject<GeoPoint> _cursorMoved = new();

    internal MapInteractionSession(MapInteractionController owner) => _owner = owner;

    public Observable<GeoPoint> Clicked => _clicked;

    public Observable<GeoPoint> CursorMoved => _cursorMoved;

    public CompositeDisposable Disposable { get; } = new();

    public string? Status
    {
        set => _owner.SetStatus(this, value);
    }

    public AsvColorKind? Accent
    {
        set => _owner.SetAccent(this, value);
    }

    public void Dispose() => _owner.End(this);

    internal void PushClick(GeoPoint point) => _clicked.OnNext(point);

    internal void PushCursor(GeoPoint cursor) => _cursorMoved.OnNext(cursor);

    internal void Complete()
    {
        Disposable.Dispose();
        _clicked.Dispose();
        _cursorMoved.Dispose();
    }
}

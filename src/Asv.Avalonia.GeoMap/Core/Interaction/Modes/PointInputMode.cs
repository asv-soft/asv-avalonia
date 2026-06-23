using Asv.Common;
using Avalonia.Input;
using R3;

namespace Asv.Avalonia.GeoMap;

public sealed class PointInputMode : IMapInteractionMode, IMapClickHandler, ICursorMoveHandler
{
    private readonly Subject<GeoPoint> _clicked = new();
    private readonly Subject<GeoPoint> _cursorMoved = new();

    public Observable<GeoPoint> Clicked => _clicked;

    public Observable<GeoPoint> CursorMoved => _cursorMoved;

    public void OnMapClick(GeoPoint point, MouseButton button, KeyModifiers modifiers) =>
        _clicked.OnNext(point);

    public void OnCursorMoved(GeoPoint cursor) => _cursorMoved.OnNext(cursor);

    public void Dispose()
    {
        _clicked.Dispose();
        _cursorMoved.Dispose();
    }
}

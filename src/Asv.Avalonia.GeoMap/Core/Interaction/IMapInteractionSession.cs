using Asv.Common;
using R3;

namespace Asv.Avalonia.GeoMap;

public interface IMapInteractionSession : IDisposable
{
    Observable<GeoPoint> Clicked { get; }

    Observable<GeoPoint> CursorMoved { get; }

    CompositeDisposable Disposable { get; }

    string? Status { set; }

    AsvColorKind? Accent { set; }
}

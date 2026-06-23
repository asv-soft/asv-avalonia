using System.Diagnostics.CodeAnalysis;
using Asv.Common;
using R3;

namespace Asv.Avalonia.GeoMap;

public interface IMapInteractionController
{
    bool IsBusy { get; }

    IReadOnlyBindableReactiveProperty<string?> Status { get; }

    IReadOnlyBindableReactiveProperty<AsvColorKind?> Accent { get; }

    bool TryBegin(
        MapInteractionRequest request,
        [MaybeNullWhen(false)] out IMapInteractionSession session
    );

    void SetStatus(IMapInteractionSession session, string? value);

    void SetAccent(IMapInteractionSession session, AsvColorKind? value);

    void End(IMapInteractionSession session);

    Observable<GeoPoint> Clicked { get; }

    Observable<GeoPoint> CursorMoved { get; }

    void AttachMap(MapItemsControl map);

    void DetachMap();
}

using System.Diagnostics.CodeAnalysis;
using R3;

namespace Asv.Avalonia.GeoMap;

public interface IMapInteractionController
{
    IReadOnlyBindableReactiveProperty<string?> Status { get; }

    IReadOnlyBindableReactiveProperty<AsvColorKind?> Accent { get; }

    bool IsBusy { get; }

    bool TryBegin([MaybeNullWhen(false)] out IMapInteractionSession session);

    void AttachMap(MapItemsControl map);

    void DetachMap();
}

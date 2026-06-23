using ObservableCollections;
using R3;

namespace Asv.Avalonia.GeoMap;

/// <summary>Owns the single active <see cref="IMapInteractionMode"/> for a map and routes map input to it.</summary>
public interface IMapInteractionService
{
    bool IsAttached { get; }

    BindableReactiveProperty<IMapInteractionMode> ActiveMode { get; }

    IReadOnlyBindableReactiveProperty<string?> StatusText { get; }

    IReadOnlyBindableReactiveProperty<AsvColorKind> Accent { get; }

    void Activate(IMapInteractionMode mode);

    void Deactivate();

    void AttachMap(MapItemsControl map, ObservableList<IMapAnchor> anchors);

    void DetachMap();
}

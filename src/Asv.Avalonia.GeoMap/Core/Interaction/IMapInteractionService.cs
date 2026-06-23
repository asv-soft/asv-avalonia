using System.Diagnostics.CodeAnalysis;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.GeoMap;

/// <summary>Owns the single active <see cref="IMapInteractionMode"/> for a map and routes map input to it.</summary>
public interface IMapInteractionService
{
    bool IsAttached { get; }

    IReadOnlyBindableReactiveProperty<IMapInteractionMode> ActiveMode { get; }

    BindableReactiveProperty<string?> Status { get; }

    BindableReactiveProperty<AsvColorKind?> Accent { get; }

    void AddMode(IMapInteractionMode mode);

    bool TryActivate<TMode>(
        [MaybeNullWhen(false)] out TMode mode,
        [MaybeNullWhen(false)] out CompositeDisposable scope
    )
        where TMode : class, IMapInteractionMode;

    void Deactivate();

    void AttachMap(MapItemsControl map, ObservableList<IMapAnchor> anchors);

    void DetachMap();
}

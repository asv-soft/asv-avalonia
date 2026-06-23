using ObservableCollections;
using R3;

namespace Asv.Avalonia.GeoMap;

/// <summary>No-op <see cref="IMapInteractionService"/> for design-time and when no map is attached.</summary>
public sealed class NullMapInteractionService : IMapInteractionService
{
    public static IMapInteractionService Instance { get; } = new NullMapInteractionService();

    private readonly BindableReactiveProperty<string?> _statusText = new();

    public IMapInteractionMode? ActiveMode => null;

    public bool IsAttached => false;

    public IReadOnlyBindableReactiveProperty<string?> StatusText => _statusText;

    public void Activate(IMapInteractionMode mode) { }

    public void Deactivate() { }

    public void AttachMap(MapItemsControl map, ObservableList<IMapAnchor> anchors) { }

    public void DetachMap() { }
}

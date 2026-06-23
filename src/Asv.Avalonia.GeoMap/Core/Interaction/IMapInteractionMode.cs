using Asv.Common;
using Avalonia.Input;
using ObservableCollections;

namespace Asv.Avalonia.GeoMap;

/// <summary>A map interaction mode. Only one is active at a time and interprets map input.</summary>
public interface IMapInteractionMode
{
    string Id { get; }

    string? StatusText { get; }

    void OnActivated(IMapInteractionContext context);

    void OnDeactivated();

    void OnMapClick(GeoPoint point, MouseButton button, KeyModifiers modifiers);

    void OnCursorMoved(GeoPoint cursor);
}

public interface IMapInteractionContext
{
    ObservableList<IMapAnchor> Anchors { get; }

    void RequestExit();
}

using Asv.Common;
using Avalonia.Input;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.GeoMap;

/// <summary>A map interaction mode. Only one is active at a time; the controller routes map input to it.</summary>
public interface IMapInteractionMode : IDisposable
{
    void OnActivated(IMapInteractionContext context, CompositeDisposable until) { }
}

public interface IMapInteractionContext
{
    ObservableList<IMapAnchor> Anchors { get; }

    void RequestExit();
}

public interface IMapClickHandler
{
    void OnMapClick(GeoPoint point, MouseButton button, KeyModifiers modifiers);
}

public interface ICursorMoveHandler
{
    void OnCursorMoved(GeoPoint cursor);
}

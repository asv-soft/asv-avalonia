using Avalonia.Input;
using Avalonia.Interactivity;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.GeoMap;

/// <summary>Default <see cref="IMapInteractionService"/> implementation.</summary>
public sealed class MapInteractionController : IMapInteractionService, IMapInteractionContext
{
    private readonly BindableReactiveProperty<string?> _statusText = new();

    private MapItemsControl? _map;
    private ObservableList<IMapAnchor>? _anchors;

    public IMapInteractionMode? ActiveMode { get; private set; }

    public bool IsAttached => _map is not null;

    public IReadOnlyBindableReactiveProperty<string?> StatusText => _statusText;

    ObservableList<IMapAnchor> IMapInteractionContext.Anchors =>
        _anchors ?? throw new InvalidOperationException("Map interaction: no map attached.");

    void IMapInteractionContext.RequestExit() => Deactivate();

    public void Activate(IMapInteractionMode mode)
    {
        Deactivate();
        ActiveMode = mode;
        mode.OnActivated(this);
        _statusText.Value = mode.StatusText;
    }

    public void Deactivate()
    {
        var mode = ActiveMode;
        if (mode is null)
        {
            return;
        }

        ActiveMode = null;
        mode.OnDeactivated();
        _statusText.Value = null;
    }

    public void AttachMap(MapItemsControl map, ObservableList<IMapAnchor> anchors)
    {
        DetachMap();

        _map = map;
        _anchors = anchors;

        map.AddHandler(MapItemsControl.MapClickEvent, OnMapClick);
        map.AddHandler(
            InputElement.PointerMovedEvent,
            OnPointerMoved,
            RoutingStrategies.Bubble,
            handledEventsToo: true
        );

        map.AddHandler(
            InputElement.PointerPressedEvent,
            OnPointerPressed,
            RoutingStrategies.Bubble,
            handledEventsToo: true
        );
        map.AddHandler(
            InputElement.KeyDownEvent,
            OnKeyDown,
            RoutingStrategies.Bubble,
            handledEventsToo: true
        );
    }

    public void DetachMap()
    {
        Deactivate();

        if (_map is not null)
        {
            _map.RemoveHandler(MapItemsControl.MapClickEvent, OnMapClick);
            _map.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
            _map.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
            _map.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        }

        _map = null;
        _anchors = null;
    }

    private void OnMapClick(object? sender, MapClickEventArgs e)
    {
        var mode = ActiveMode;
        if (mode is null)
        {
            return;
        }

        mode.OnMapClick(e.Point, e.Button, e.Modifiers);
        e.Handled = true;
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (ActiveMode is null || _map is null)
        {
            return;
        }

        ActiveMode.OnCursorMoved(_map.CursorPosition);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ActiveMode is null || _map is null)
        {
            return;
        }

        if (e.GetCurrentPoint(_map).Properties.IsRightButtonPressed)
        {
            Deactivate();
            e.Handled = true;
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (ActiveMode is null)
        {
            return;
        }

        if (e.Key == Key.Escape)
        {
            Deactivate();
            e.Handled = true;
        }
    }
}

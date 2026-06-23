using Avalonia.Input;
using Avalonia.Interactivity;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.GeoMap;

public sealed class MapInteractionController
    : IMapInteractionService,
        IMapInteractionContext,
        IDisposable
{
    private readonly BindableReactiveProperty<string?> _statusText = new();
    private readonly BindableReactiveProperty<AsvColorKind> _accent = new(AsvColorKind.Info5);
    private readonly CompositeDisposable _disposable = new();

    private MapItemsControl? _map;
    private ObservableList<IMapAnchor>? _anchors;
    private CompositeDisposable? _until;

    public MapInteractionController()
    {
        ActiveMode.Subscribe(OnActiveModeChanged).AddTo(_disposable);
        ActiveMode.AddTo(_disposable);
        _statusText.AddTo(_disposable);
        _accent.AddTo(_disposable);
    }

    public bool IsAttached => _map is not null;

    public BindableReactiveProperty<IMapInteractionMode> ActiveMode { get; } =
        new(NavigateMode.Instance);

    public IReadOnlyBindableReactiveProperty<string?> StatusText => _statusText;

    public IReadOnlyBindableReactiveProperty<AsvColorKind> Accent => _accent;

    ObservableList<IMapAnchor> IMapInteractionContext.Anchors =>
        _anchors ?? throw new InvalidOperationException("Map interaction: no map attached.");

    void IMapInteractionContext.RequestExit() => Deactivate();

    public void Activate(IMapInteractionMode mode) => ActiveMode.Value = mode;

    public void Deactivate() => ActiveMode.Value = NavigateMode.Instance;

    private void OnActiveModeChanged(IMapInteractionMode mode)
    {
        _until?.Dispose();
        _until = null;
        _statusText.Value = null;

        var until = new CompositeDisposable();
        _until = until;
        _accent.Value = mode.Accent;
        mode.StatusText.Subscribe(text => _statusText.Value = text).AddTo(until);
        mode.OnActivated(this, until);
    }

    public void AttachMap(MapItemsControl map, ObservableList<IMapAnchor> anchors)
    {
        DetachMap();

        _map = map;
        _anchors = anchors;

        map.AddHandler(MapItemsControl.MapClickEvent, OnMapClick);
        map.AddHandler(
            InputElement.PointerMovedEvent,
            OnMapPointerMoved,
            RoutingStrategies.Bubble,
            handledEventsToo: true
        );
        map.AddHandler(
            InputElement.PointerPressedEvent,
            OnMapPointerPressed,
            RoutingStrategies.Bubble,
            handledEventsToo: true
        );
        map.AddHandler(
            InputElement.KeyDownEvent,
            OnMapKeyDown,
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
            _map.RemoveHandler(InputElement.PointerMovedEvent, OnMapPointerMoved);
            _map.RemoveHandler(InputElement.PointerPressedEvent, OnMapPointerPressed);
            _map.RemoveHandler(InputElement.KeyDownEvent, OnMapKeyDown);
        }

        _map = null;
        _anchors = null;
    }

    private void OnMapClick(object? sender, MapClickEventArgs e)
    {
        if (ActiveMode.Value is IMapClickHandler handler)
        {
            handler.OnMapClick(e.Point, e.Button, e.Modifiers);
            e.Handled = true;
        }
    }

    private void OnMapPointerMoved(object? sender, PointerEventArgs e)
    {
        if (ActiveMode.Value is ICursorMoveHandler handler && _map is not null)
        {
            handler.OnCursorMoved(_map.CursorPosition);
        }
    }

    private void OnMapPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (ReferenceEquals(ActiveMode.Value, NavigateMode.Instance) || _map is null)
        {
            return;
        }

        if (e.GetCurrentPoint(_map).Properties.IsRightButtonPressed)
        {
            Deactivate();
            e.Handled = true;
        }
    }

    private void OnMapKeyDown(object? sender, KeyEventArgs e)
    {
        if (ReferenceEquals(ActiveMode.Value, NavigateMode.Instance))
        {
            return;
        }

        if (e.Key == Key.Escape)
        {
            Deactivate();
            e.Handled = true;
        }
    }

    public void Dispose()
    {
        DetachMap();
        _until?.Dispose();
        _until = null;
        _disposable.Dispose();
    }
}

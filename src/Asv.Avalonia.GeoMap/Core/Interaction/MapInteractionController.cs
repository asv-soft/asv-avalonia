using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
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
    private readonly NavigateMode _navigateMode = new();
    private readonly List<IMapInteractionMode> _modes = [];
    private readonly BindableReactiveProperty<IMapInteractionMode> _activeMode;
    private readonly CompositeDisposable _disposable = new();

    private MapItemsControl? _map;
    private TopLevel? _topLevel;
    private ObservableList<IMapAnchor>? _anchors;
    private CompositeDisposable _until = new();

    public MapInteractionController()
    {
        _modes.Add(_navigateMode);
        _activeMode = new BindableReactiveProperty<IMapInteractionMode>(_navigateMode);
        _activeMode.AddTo(_disposable);
        Status.AddTo(_disposable);
        Accent.AddTo(_disposable);
    }

    public bool IsAttached => _map is not null;

    public IReadOnlyBindableReactiveProperty<IMapInteractionMode> ActiveMode => _activeMode;

    public BindableReactiveProperty<string?> Status { get; } = new();

    public BindableReactiveProperty<AsvColorKind?> Accent { get; } = new();

    ObservableList<IMapAnchor> IMapInteractionContext.Anchors =>
        _anchors ?? throw new InvalidOperationException("No map attached.");

    void IMapInteractionContext.RequestExit() => Deactivate();

    public void AddMode(IMapInteractionMode mode) => _modes.Add(mode);

    public bool TryActivate<TMode>(
        [MaybeNullWhen(false)] out TMode mode,
        [MaybeNullWhen(false)] out CompositeDisposable scope
    )
        where TMode : class, IMapInteractionMode
    {
        var found = _modes.OfType<TMode>().FirstOrDefault();
        if (found is null || !IsAttached)
        {
            mode = null;
            scope = null;

            return false;
        }

        mode = found;
        scope = Activate(found);
        return true;
    }

    public void Deactivate() => Activate(_navigateMode);

    private CompositeDisposable Activate(IMapInteractionMode mode)
    {
        _until.Dispose();
        _until = new CompositeDisposable();
        Status.Value = null;
        Accent.Value = null;
        _activeMode.Value = mode;
        mode.OnActivated(this, _until);
        return _until;
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

        _topLevel = TopLevel.GetTopLevel(map);
        _topLevel?.AddHandler(
            InputElement.KeyDownEvent,
            OnKeyDown,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble,
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
        }

        _topLevel?.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        _topLevel = null;
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

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && !ReferenceEquals(ActiveMode.Value, _navigateMode))
        {
            Deactivate();
            e.Handled = true;
        }
    }

    public void Dispose()
    {
        DetachMap();

        _until.Dispose();

        foreach (var mode in _modes)
        {
            mode.Dispose();
        }

        _disposable.Dispose();
    }
}

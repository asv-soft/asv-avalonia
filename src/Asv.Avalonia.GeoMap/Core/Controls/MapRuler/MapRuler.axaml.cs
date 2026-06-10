using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

/// <summary>
/// <para>
/// Map ruler: two draggable endpoints connected by a Google Maps-like polyline
/// with a live distance readout in the ruler control.
/// </para>
/// <para>
/// The ruler's persistent state is exposed via <see cref="StartPoint"/> /
/// <see cref="StopPoint"/> styled properties. Both not null = ruler shown; either
/// null = ruler hidden. The ruler treats writes from the picking flow, the user's
/// drag of an endpoint, and an external host writer identically — they all go
/// through the same property-changed handler. The <see cref="RulerChanged"/>
/// routed event reports any meaningful change (shown / hidden / endpoint moved)
/// so a host can persist the points via its own layout service. The control
/// itself knows nothing about persistence.
/// </para>
/// <para>
/// The ruler attaches a bubbling handler for <see cref="MapItemsControl.MapClickEvent"/>
/// to the explicit <see cref="TargetMap"/> input element.
/// </para>
/// <para>
/// Engagement cycle (driven by toggle button + map clicks):
/// <c>Idle</c> → <c>PickingStart</c> → <c>PickingEnd</c> → <c>Active</c> → <c>Idle</c>.
/// A click on the toggle while in any non-Idle state cancels the ruler.
/// </para>
/// </summary>
public sealed partial class MapRuler : UserControl
{
    private const string ToggleControlName = "PART_Toggle";
    private const string FirstAnchorName = "ruler_start";
    private const string SecondAnchorName = "ruler_stop";
    private const string LineAnchorName = "ruler_line";
    private const string LineHaloAnchorName = "ruler_line_halo";
    private const double RulerStrokeThickness = 3.0;
    private const double RulerHaloStrokeThickness = 7.0;
    private static readonly IBrush RulerBrush = SolidColorBrush.Parse("#1A73E8");
    private static readonly IBrush RulerHaloBrush = Brushes.White;

    #region State

    private enum RulerState
    {
        Idle,
        PickingStart,
        PickingEnd,
        Active,
    }

    private RulerState _state = RulerState.Idle;
    private GeoPoint? _firstPoint;
    private IMapAnchor? _start;
    private IMapAnchor? _stop;
    private IMapAnchor? _line;
    private IMapAnchor? _lineHalo;
    private DisposableBag _activeSubs;
    private IList<IMapAnchor>? _ownedAnchorsList;
    private InputElement? _clickSurface;
    private MapItemsControl? _mapControl;
    private readonly ToggleButton _toggle;
    private bool _isAttached;
    private bool _isPreview;
    private bool _lastFiredShown;

    #endregion

    #region Init

    static MapRuler()
    {
        TargetMapProperty.Changed.AddClassHandler<MapRuler>(
            (ruler, _) => ruler.AttachToClickSurface()
        );

        AnchorsProperty.Changed.AddClassHandler<MapRuler>((ruler, _) => ruler.OnPointsChanged());

        UnitServiceProperty.Changed.AddClassHandler<MapRuler>(
            (ruler, _) => ruler.UpdateDistanceLabel()
        );

        StartPointProperty.Changed.AddClassHandler<MapRuler>((ruler, _) => ruler.OnPointsChanged());

        StopPointProperty.Changed.AddClassHandler<MapRuler>((ruler, _) => ruler.OnPointsChanged());
    }

    public MapRuler()
    {
        InitializeComponent();

        _toggle = this.GetControl<ToggleButton>(ToggleControlName);
    }

    #endregion

    #region Attach / detach

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        _isAttached = true;
        AttachToClickSurface();
        OnPointsChanged();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        TearDownVisuals();
        ResetPicking();
        DetachFromClickSurface();

        base.OnDetachedFromVisualTree(e);
    }

    private void AttachToClickSurface()
    {
        DetachFromClickSurface();

        _clickSurface = TargetMap;
        _mapControl = ResolveMapControl(_clickSurface);
        _clickSurface?.AddHandler(MapItemsControl.MapClickEvent, OnMapClick);
        _clickSurface?.AddHandler(
            InputElement.PointerMovedEvent,
            OnMapPointerMoved,
            RoutingStrategies.Bubble,
            true
        );
    }

    private void DetachFromClickSurface()
    {
        _clickSurface?.RemoveHandler(MapItemsControl.MapClickEvent, OnMapClick);
        _clickSurface?.RemoveHandler(InputElement.PointerMovedEvent, OnMapPointerMoved);
        _clickSurface = null;
        _mapControl = null;
    }

    private static MapItemsControl? ResolveMapControl(InputElement? target)
    {
        if (target is MapItemsControl mapControl)
        {
            return mapControl;
        }

        return target is Visual visual
            ? visual.GetVisualDescendants().OfType<MapItemsControl>().FirstOrDefault()
            : null;
    }

    #endregion

    #region Toggle button

    private void OnToggleClick(object? sender, RoutedEventArgs e)
    {
        switch (_state)
        {
            case RulerState.Idle:
                BeginPicking();
                break;
            case RulerState.PickingStart:
            case RulerState.PickingEnd:
                ClearRuler();
                break;
            case RulerState.Active:
            default:
                ClearRuler();
                break;
        }

        SyncToggleVisual();
    }

    private void OnClearClick(object? sender, RoutedEventArgs e)
    {
        ClearRuler();
        e.Handled = true;
    }

    private void SyncToggleVisual()
    {
        _toggle.IsChecked = _state != RulerState.Idle;
    }

    private void BeginPicking()
    {
        DistanceText = null;
        _state = RulerState.PickingStart;
        PromptText = RS.MapRuler_PickStart;
    }

    private void ClearRuler()
    {
        if (_isPreview)
        {
            TearDownVisuals();
        }

        StartPoint = null;
        StopPoint = null;
        ResetPicking();
    }

    private void ResetPicking()
    {
        _firstPoint = null;
        _isPreview = false;
        _state = RulerState.Idle;
        PromptText = null;
        DistanceText = null;
        SyncToggleVisual();
    }

    #endregion

    #region Map click

    private void OnMapClick(object? sender, MapClickEventArgs e)
    {
        switch (_state)
        {
            case RulerState.PickingStart:
                _firstPoint = e.Point;
                _state = RulerState.PickingEnd;
                PromptText = RS.MapRuler_PickEnd;
                BuildVisuals(e.Point, e.Point, true);
                break;
            case RulerState.PickingEnd:
                if (_firstPoint is null)
                {
                    ResetPicking();
                    return;
                }

                var first = _firstPoint.Value;
                _firstPoint = null;
                StartPoint = first;
                StopPoint = e.Point;
                break;
            case RulerState.Active:
            case RulerState.Idle:
            default:
                return;
        }

        e.Handled = true;

        SyncToggleVisual();
    }

    #endregion

    private void OnMapPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_state != RulerState.PickingEnd || _firstPoint is null || _mapControl is null)
        {
            return;
        }

        UpdatePreviewStop(_mapControl.CursorPosition);
    }

    #region Build / tear down visuals

    private void BuildVisuals(GeoPoint startLocation, GeoPoint stopLocation, bool isPreview)
    {
        TearDownVisuals();
        _isPreview = isPreview;

        _start = CreateEndpoint(FirstAnchorName, startLocation);
        _stop = CreateEndpoint(SecondAnchorName, stopLocation);
        _lineHalo = CreateLine(
            LineHaloAnchorName,
            startLocation,
            stopLocation,
            RulerHaloBrush,
            RulerHaloStrokeThickness
        );
        _line = CreateLine(
            LineAnchorName,
            startLocation,
            stopLocation,
            RulerBrush,
            RulerStrokeThickness
        );
        ApplyVisualMode(isPreview);

        _start
            .ObservePropertyChanged(x => x.Location)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(p => OnEndpointMoved(true, p))
            .AddTo(ref _activeSubs);

        _stop
            .ObservePropertyChanged(x => x.Location)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(p => OnEndpointMoved(false, p))
            .AddTo(ref _activeSubs);

        UnitService
            .Units[DistanceUnit.Id]
            .CurrentUnitItem.ObserveOnUIThreadDispatcher()
            .Subscribe(_ => UpdateDistanceLabel())
            .AddTo(ref _activeSubs);

        _ownedAnchorsList = Anchors;
        _ownedAnchorsList?.Add(_lineHalo);
        _ownedAnchorsList?.Add(_line);
        _ownedAnchorsList?.Add(_start);
        _ownedAnchorsList?.Add(_stop);

        UpdateDistanceLabel();
    }

    private void ApplyVisualMode(bool isPreview)
    {
        if (_start is not null)
        {
            _start.IsReadOnly = isPreview;
            _start.CanDragWithoutModifier = !isPreview;
            _start.IsAnnotationVisible = false;
        }

        if (_stop is not null)
        {
            _stop.IsReadOnly = isPreview;
            _stop.CanDragWithoutModifier = !isPreview;
            _stop.IsAnnotationVisible = false;
        }
    }

    private void OnEndpointMoved(bool isStart, GeoPoint point)
    {
        if (_isPreview)
        {
            if (isStart)
            {
                _firstPoint = point;
            }

            UpdateLineFromEndpoints();
            UpdateDistanceLabel();
            return;
        }

        if (isStart)
        {
            StartPoint = point;
        }
        else
        {
            StopPoint = point;
        }
    }

    private void UpdatePreviewStop(GeoPoint stopLocation)
    {
        if (_firstPoint is not { } startLocation)
        {
            return;
        }

        if (_start is null || _stop is null)
        {
            BuildVisuals(startLocation, stopLocation, true);
            return;
        }

        _stop.Location = stopLocation;
        UpdateLine(startLocation, stopLocation);
        UpdateDistanceLabel();
    }

    private void TearDownVisuals()
    {
        _activeSubs.Dispose();
        _activeSubs = default;

        if (_ownedAnchorsList is not null)
        {
            if (_lineHalo is not null)
            {
                _ownedAnchorsList.Remove(_lineHalo);
            }

            if (_line is not null)
            {
                _ownedAnchorsList.Remove(_line);
            }

            if (_start is not null)
            {
                _ownedAnchorsList.Remove(_start);
            }

            if (_stop is not null)
            {
                _ownedAnchorsList.Remove(_stop);
            }
        }

        _ownedAnchorsList = null;
        _start = null;
        _stop = null;
        _line = null;
        _lineHalo = null;
        _isPreview = false;
        DistanceText = null;
    }

    #endregion

    #region Single state-change handler

    private void OnPointsChanged()
    {
        var start = StartPoint;
        var stop = StopPoint;

        if (start is { } startPoint && stop is { } stopPoint)
        {
            if (MatchesCurrentVisuals(startPoint, stopPoint))
            {
                UpdateLine(startPoint, stopPoint);
                _isPreview = false;
                ApplyVisualMode(false);
                UpdateDistanceLabel();
            }
            else if (_isAttached)
            {
                BuildVisuals(startPoint, stopPoint, false);
            }

            _state = RulerState.Active;
            _firstPoint = null;
            PromptText = null;
            SyncToggleVisual();
        }
        else
        {
            if (
                _start is not null
                || _stop is not null
                || _line is not null
                || _lineHalo is not null
            )
            {
                TearDownVisuals();
            }

            if (_state == RulerState.Active)
            {
                ResetPicking();
            }
        }

        var bothSet = start.HasValue && stop.HasValue;

        if (bothSet || _lastFiredShown)
        {
            RaiseEvent(new RulerChangedEventArgs(RulerChangedEvent, start, stop));
            _lastFiredShown = bothSet;
        }
    }

    private bool MatchesCurrentVisuals(GeoPoint start, GeoPoint stop)
    {
        return _start is not null
            && _stop is not null
            && ReferenceEquals(_ownedAnchorsList, Anchors)
            && _start.Location.Equals(start)
            && _stop.Location.Equals(stop);
    }

    #endregion

    #region Line geometry

    private void UpdateLineFromEndpoints()
    {
        if (_start is null || _stop is null)
        {
            return;
        }

        UpdateLine(_start.Location, _stop.Location);
    }

    private void UpdateLine(GeoPoint start, GeoPoint stop)
    {
        UpdateLine(_lineHalo, start, stop);
        UpdateLine(_line, start, stop);
    }

    private static void UpdateLine(IMapAnchor? line, GeoPoint start, GeoPoint stop)
    {
        if (line is null)
        {
            return;
        }

        if (line.Polygon.Count < 2)
        {
            line.Polygon.Clear();
            line.Polygon.Add(start);
            line.Polygon.Add(stop);
            return;
        }

        line.Polygon[0] = start;
        line.Polygon[1] = stop;
    }

    #endregion

    #region Distance label

    private void UpdateDistanceLabel()
    {
        if (_start is null || _stop is null)
        {
            DistanceText = null;
            return;
        }

        var d = GeoMath.Distance(_start.Location, _stop.Location);
        var unit = UnitService.Units[DistanceUnit.Id].CurrentUnitItem.Value;
        DistanceText = unit.PrintFromSiWithUnits(d, "F1");
    }

    #endregion

    #region Endpoint factory

    private static IMapAnchor CreateEndpoint(string id, GeoPoint location) =>
        new MapAnchor(id)
        {
            Header = string.Empty,
            Location = location,
            Icon = MaterialIconKind.Circle,
            IconSize = 18,
            IconColor = AsvColorKind.Info5,
            CenterX = new HorizontalOffset(HorizontalOffsetEnum.Center, 0),
            CenterY = new VerticalOffset(VerticalOffsetEnum.Center, 0),
            IsAnnotationVisible = false,
            IsReadOnly = false,
        };

    private static IMapAnchor CreateLine(
        string id,
        GeoPoint start,
        GeoPoint stop,
        IBrush brush,
        double thickness
    )
    {
        var line = new MapAnchor(id)
        {
            Header = string.Empty,
            IsReadOnly = true,
            IsAnnotationVisible = false,
            IsPolygonClosed = false,
            IconSize = 0,
            PolygonPen = new Pen(brush, thickness),
        };
        line.Polygon.Add(start);
        line.Polygon.Add(stop);
        return line;
    }

    #endregion
}

using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

/// <summary>
/// <para>
/// Map ruler: two draggable endpoints connected by a dashed polyline with a live
/// distance readout shown as the "stop" anchor's annotation.
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
    private const string FirstAnchorName = "ruler.start";
    private const string SecondAnchorName = "ruler.stop";
    private const string LineAnchorName = "ruler.line";
    private const double RulerStrokeThickness = 4.0;
    private static readonly IBrush RulerBrush = Brushes.Indigo;
    private static readonly double[] RulerPattern = [2.0, 2.0];

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
    private MapAnchor<IMapAnchor>? _start;
    private MapAnchor<IMapAnchor>? _stop;
    private MapAnchor<IMapAnchor>? _line;
    private DisposableBag _activeSubs;
    private IList<IMapAnchor>? _ownedAnchorsList;
    private InputElement? _clickSurface;
    private readonly ToggleButton _toggle;
    private bool _isAttached;
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
        _clickSurface?.AddHandler(MapItemsControl.MapClickEvent, OnMapClick);
    }

    private void DetachFromClickSurface()
    {
        _clickSurface?.RemoveHandler(MapItemsControl.MapClickEvent, OnMapClick);
        _clickSurface = null;
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
                ResetPicking();
                break;
            case RulerState.Active:
            default:
                StartPoint = null;
                StopPoint = null;
                break;
        }

        SyncToggleVisual();
    }

    private void SyncToggleVisual()
    {
        _toggle.IsChecked = _state != RulerState.Idle;
    }

    private void BeginPicking()
    {
        _state = RulerState.PickingStart;
        PromptText = RS.MapRuler_PickStart;
    }

    private void ResetPicking()
    {
        _firstPoint = null;
        _state = RulerState.Idle;
        PromptText = null;
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
            case RulerState.Idle:
            case RulerState.Active:
            default:
                return;
        }

        e.Handled = true;

        SyncToggleVisual();
    }

    #endregion

    #region Build / tear down visuals

    private void BuildVisuals(GeoPoint startLocation, GeoPoint stopLocation)
    {
        TearDownVisuals();

        _start = CreateEndpoint(FirstAnchorName, LoggerFactory, startLocation);
        _stop = CreateEndpoint(SecondAnchorName, LoggerFactory, stopLocation);
        _start.IsAnnotationVisible = false;

        var linePen = new Pen(RulerBrush, RulerStrokeThickness)
        {
            DashStyle = new DashStyle(RulerPattern, 0),
        };
        _line = new MapAnchor<IMapAnchor>(LineAnchorName, LoggerFactory)
        {
            Title = string.Empty,
            IsReadOnly = true,
            IsAnnotationVisible = false,
            IsPolygonClosed = false,
            IconSize = 0,
            PolygonPen = linePen,
        };
        _line.Polygon.Add(startLocation);
        _line.Polygon.Add(stopLocation);

        _start
            .ObservePropertyChanged(x => x.Location)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(p => StartPoint = p)
            .AddTo(ref _activeSubs);

        _stop
            .ObservePropertyChanged(x => x.Location)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(p => StopPoint = p)
            .AddTo(ref _activeSubs);

        UnitService
            .Units[DistanceUnit.Id]
            .CurrentUnitItem.ObserveOnUIThreadDispatcher()
            .Subscribe(_ => UpdateDistanceLabel())
            .AddTo(ref _activeSubs);

        _ownedAnchorsList = Anchors;
        _ownedAnchorsList?.Add(_start);
        _ownedAnchorsList?.Add(_stop);
        _ownedAnchorsList?.Add(_line);

        UpdateDistanceLabel();
    }

    private void TearDownVisuals()
    {
        _activeSubs.Dispose();
        _activeSubs = default;

        if (_ownedAnchorsList is not null)
        {
            if (_start is not null)
            {
                _ownedAnchorsList.Remove(_start);
            }

            if (_stop is not null)
            {
                _ownedAnchorsList.Remove(_stop);
            }

            if (_line is not null)
            {
                _ownedAnchorsList.Remove(_line);
            }
        }

        _ownedAnchorsList = null;
        _start = null;
        _stop = null;
        _line = null;
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
                if (_line is not null)
                {
                    _line.Polygon[0] = startPoint;
                    _line.Polygon[1] = stopPoint;
                }

                UpdateDistanceLabel();
            }
            else if (_isAttached)
            {
                BuildVisuals(startPoint, stopPoint);
            }

            _state = RulerState.Active;
            _firstPoint = null;
            PromptText = null;
            SyncToggleVisual();
        }
        else
        {
            if (_start is not null || _stop is not null || _line is not null)
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

    #region Distance label

    private void UpdateDistanceLabel()
    {
        if (_start is null || _stop is null)
        {
            return;
        }

        var d = GeoMath.Distance(_start.Location, _stop.Location);
        var unit = UnitService.Units[DistanceUnit.Id].CurrentUnitItem.Value;
        _stop.Title = unit.PrintFromSiWithUnits(d, "F1");
    }

    #endregion

    #region Endpoint factory

    private static MapAnchor<IMapAnchor> CreateEndpoint(
        NavigationId id,
        ILoggerFactory loggerFactory,
        GeoPoint location
    ) =>
        new(id, loggerFactory)
        {
            Location = location,
            Icon = MaterialIconKind.MapMarker,
            IconSize = 40,
            IconColor = AsvColorKind.Info4,
            CenterX = new HorizontalOffset(HorizontalOffsetEnum.Center, 0),
            CenterY = new VerticalOffset(VerticalOffsetEnum.Bottom, 0),
            IsAnnotationVisible = true,
            IsReadOnly = false,
        };

    #endregion
}

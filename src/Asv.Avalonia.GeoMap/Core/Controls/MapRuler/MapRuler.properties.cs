using Asv.Common;
using Avalonia;
using Avalonia.Interactivity;

namespace Asv.Avalonia.GeoMap;

public partial class MapRuler
{
    #region Map

    public static readonly StyledProperty<IMap?> MapProperty = AvaloniaProperty.Register<
        MapRuler,
        IMap?
    >(nameof(Map));

    public IMap? Map
    {
        get => GetValue(MapProperty);
        set => SetValue(MapProperty, value);
    }

    #endregion

    #region UnitService

    public static readonly StyledProperty<IUnitService?> UnitServiceProperty =
        AvaloniaProperty.Register<MapRuler, IUnitService?>(nameof(UnitService));

    public IUnitService? UnitService
    {
        get => GetValue(UnitServiceProperty);
        set => SetValue(UnitServiceProperty, value);
    }

    #endregion

    #region DistanceText

    public static readonly DirectProperty<MapRuler, string?> DistanceTextProperty =
        AvaloniaProperty.RegisterDirect<MapRuler, string?>(
            nameof(DistanceText),
            o => o.DistanceText
        );

    public string? DistanceText
    {
        get;
        private set => SetAndRaise(DistanceTextProperty, ref field, value);
    }

    #endregion

    #region StartPoint / StopPoint

    public static readonly StyledProperty<GeoPoint?> StartPointProperty = AvaloniaProperty.Register<
        MapRuler,
        GeoPoint?
    >(nameof(StartPoint));

    public GeoPoint? StartPoint
    {
        get => GetValue(StartPointProperty);
        set => SetValue(StartPointProperty, value);
    }

    public static readonly StyledProperty<GeoPoint?> StopPointProperty = AvaloniaProperty.Register<
        MapRuler,
        GeoPoint?
    >(nameof(StopPoint));

    public GeoPoint? StopPoint
    {
        get => GetValue(StopPointProperty);
        set => SetValue(StopPointProperty, value);
    }

    #endregion

    #region RulerChanged

    public static readonly RoutedEvent<RulerChangedEventArgs> RulerChangedEvent =
        RoutedEvent.Register<MapRuler, RulerChangedEventArgs>(
            nameof(RulerChanged),
            RoutingStrategies.Bubble
        );

    public event EventHandler<RulerChangedEventArgs>? RulerChanged
    {
        add => AddHandler(RulerChangedEvent, value);
        remove => RemoveHandler(RulerChangedEvent, value);
    }

    #endregion
}

public sealed class RulerChangedEventArgs : RoutedEventArgs
{
    public RulerChangedEventArgs(RoutedEvent routedEvent, GeoPoint? start, GeoPoint? stop)
        : base(routedEvent)
    {
        Start = start;
        Stop = stop;
    }

    public GeoPoint? Start { get; }
    public GeoPoint? Stop { get; }
}

using Asv.Common;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public partial class MapRuler
{
    #region TargetMap

    public static readonly StyledProperty<InputElement> TargetMapProperty =
        AvaloniaProperty.Register<MapRuler, InputElement>(nameof(TargetMap));

    public InputElement TargetMap
    {
        get => GetValue(TargetMapProperty);
        set => SetValue(TargetMapProperty, value);
    }

    #endregion

    #region Anchors

    public static readonly StyledProperty<IList<IMapAnchor>> AnchorsProperty =
        AvaloniaProperty.Register<MapRuler, IList<IMapAnchor>>(nameof(Anchors));

    public IList<IMapAnchor> Anchors
    {
        get => GetValue(AnchorsProperty);
        set => SetValue(AnchorsProperty, value);
    }

    #endregion

    #region UnitService

    public static readonly StyledProperty<IUnitService> UnitServiceProperty =
        AvaloniaProperty.Register<MapRuler, IUnitService>(nameof(UnitService));

    public IUnitService UnitService
    {
        get => GetValue(UnitServiceProperty);
        set => SetValue(UnitServiceProperty, value);
    }

    #endregion

    #region LoggerFactory

    public static readonly StyledProperty<ILoggerFactory> LoggerFactoryProperty =
        AvaloniaProperty.Register<MapRuler, ILoggerFactory>(nameof(LoggerFactory));

    public ILoggerFactory LoggerFactory
    {
        get => GetValue(LoggerFactoryProperty);
        set => SetValue(LoggerFactoryProperty, value);
    }

    #endregion

    #region PromptText

    public static readonly DirectProperty<MapRuler, string?> PromptTextProperty =
        AvaloniaProperty.RegisterDirect<MapRuler, string?>(nameof(PromptText), o => o.PromptText);

    public string? PromptText
    {
        get;
        private set => SetAndRaise(PromptTextProperty, ref field, value);
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

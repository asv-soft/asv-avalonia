using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using R3;

namespace Asv.Avalonia.Map;

[PseudoClasses(":pressed", ":selected", ":pointerover")]
public class MapViewItem : ContentControl, ISelectable
{
    static MapViewItem()
    {
        SelectableMixin.Attach<MapViewItem>(IsSelectedProperty);
        PressedMixin.Attach<MapViewItem>();
        CenterYProperty.Changed.Subscribe(x => RecalculateRotation(x.Sender as MapViewItem));
        CenterYProperty.Changed.Subscribe(x => RecalculateRotation(x.Sender as MapViewItem));
        BoundsProperty.Changed.Subscribe(x => RecalculateRotation(x.Sender as MapViewItem));
    }

    private static void RecalculateRotation(MapViewItem? sender)
    {
        if (sender == null)
            return;

        sender.RotationCenterX = sender.CenterX.CalculateOffset(sender.Bounds.Width);
        sender.RotationCenterY = sender.CenterY.CalculateOffset(sender.Bounds.Height);
    }

    public MapViewItem() { }

    #region CenterY

    public static readonly StyledProperty<VerticalOffset> CenterYProperty =
        AvaloniaProperty.Register<MapViewItem, VerticalOffset>(nameof(CenterY));

    public VerticalOffset CenterY
    {
        get => GetValue(CenterYProperty);
        set => SetValue(CenterYProperty, value);
    }

    #endregion

    #region CenterX

    public static readonly StyledProperty<HorizontalOffset> CenterXProperty =
        AvaloniaProperty.Register<MapViewItem, HorizontalOffset>(nameof(CenterX));

    public HorizontalOffset CenterX
    {
        get => GetValue(CenterXProperty);
        set => SetValue(CenterXProperty, value);
    }

    #endregion

    #region Location

    public static readonly StyledProperty<GeoPoint> LocationProperty = AvaloniaProperty.Register<
        MapViewItem,
        GeoPoint
    >(nameof(Location));

    public GeoPoint Location
    {
        get => GetValue(LocationProperty);
        set => SetValue(LocationProperty, value);
    }

    #endregion

    #region RotationCenterX

    private double _rotationCenterX;

    public static readonly DirectProperty<MapViewItem, double> RotationCenterXProperty =
        AvaloniaProperty.RegisterDirect<MapViewItem, double>(
            nameof(RotationCenterX),
            o => o.RotationCenterX,
            (o, v) => o.RotationCenterX = v
        );

    public double RotationCenterX
    {
        get => _rotationCenterX;
        set => SetAndRaise(RotationCenterXProperty, ref _rotationCenterX, value);
    }

    #endregion

    #region RotationCenterY

    private double _rotationCenterY;

    public static readonly DirectProperty<MapViewItem, double> RotationCenterYProperty =
        AvaloniaProperty.RegisterDirect<MapViewItem, double>(
            nameof(RotationCenterY),
            o => o.RotationCenterY,
            (o, v) => o.RotationCenterY = v
        );

    public double RotationCenterY
    {
        get => _rotationCenterY;
        set => SetAndRaise(RotationCenterYProperty, ref _rotationCenterY, value);
    }

    #endregion

    #region Rotation

    public static readonly StyledProperty<double> RotationProperty = AvaloniaProperty.Register<
        MapViewItem,
        double
    >(nameof(Rotation));

    public double Rotation
    {
        get => GetValue(RotationProperty);
        set => SetValue(RotationProperty, value);
    }

    #endregion

    protected override Size ArrangeOverride(Size finalSize)
    {
        return base.ArrangeOverride(finalSize);
    }

    /*protected override void OnPointerEntered(PointerEventArgs e)
    {
        PseudoClasses.Add(":pointerover");
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        PseudoClasses.Remove(":pointerover");
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            PseudoClasses.Add(":pressed");
        }
    }*/

    /*protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        PseudoClasses.Remove(":pressed");
    }*/

    #region IsSelected

    public static readonly StyledProperty<bool> IsSelectedProperty = AvaloniaProperty.Register<
        MapViewItem,
        bool
    >(nameof(IsSelected));

    public bool IsSelected
    {
        get => GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    #endregion
}

using Avalonia;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Avalonia;

public partial class IndicatorBase
{
    #region Brushes

    public static readonly StyledProperty<Brush> UnknownBrushProperty = AvaloniaProperty.Register<
        IndicatorBase,
        Brush
    >("UnknownBrush");

    public Brush UnknownBrush
    {
        get => GetValue(UnknownBrushProperty);
        set => SetValue(UnknownBrushProperty, value);
    }

    public static readonly StyledProperty<Brush> CriticalBrushProperty = AvaloniaProperty.Register<
        IndicatorBase,
        Brush
    >("CriticalBrush");

    public Brush CriticalBrush
    {
        get => GetValue(CriticalBrushProperty);
        set => SetValue(CriticalBrushProperty, value);
    }

    public static readonly StyledProperty<Brush> WarningBrushProperty = AvaloniaProperty.Register<
        IndicatorBase,
        Brush
    >("WarningBrush");

    public Brush WarningBrush
    {
        get => GetValue(WarningBrushProperty);
        set => SetValue(WarningBrushProperty, value);
    }

    public static readonly StyledProperty<Brush> NormalBrushProperty = AvaloniaProperty.Register<
        IndicatorBase,
        Brush
    >("NormalBrush");

    public Brush NormalBrush
    {
        get => GetValue(NormalBrushProperty);
        set => SetValue(NormalBrushProperty, value);
    }

    #endregion

    public const string ProgressDisabledPseudoclass = ":progress-disabled";
    public const string ProgressCompletedPseudoclass = ":progress-completed";
    public static readonly StyledProperty<MaterialIconKind> IconKindProperty =
        AvaloniaProperty.Register<IndicatorBase, MaterialIconKind>(
            nameof(IconKind),
            MaterialIconKind.BatteryUnknown
        );

    public MaterialIconKind IconKind
    {
        get => GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<
        IndicatorBase,
        string
    >(nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
}

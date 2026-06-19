using Avalonia;
using Avalonia.Controls.Primitives;
using Material.Icons;

namespace Asv.Avalonia;

public class TileTemplatedControl : TemplatedControl
{
    private const string RegularClass = "regular";
    private const string CompactClass = "compact";
    private const string InlineClass = "inline";

    static TileTemplatedControl()
    {
        DensityProperty.Changed.AddClassHandler<TileTemplatedControl>(
            (view, _) => view.UpdateDensityClasses()
        );
    }

    public TileTemplatedControl()
    {
        UpdateDensityClasses();
    }

    public static readonly StyledProperty<MaterialIconKind> IconProperty =
        AvaloniaProperty.Register<TileTemplatedControl, MaterialIconKind>(
            nameof(Icon),
            MaterialIconKind.Telecoil
        );

    public MaterialIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string?> ShortHeaderProperty = AvaloniaProperty.Register<
        TileTemplatedControl,
        string?
    >(nameof(ShortHeader));

    public string? ShortHeader
    {
        get => GetValue(ShortHeaderProperty);
        set => SetValue(ShortHeaderProperty, value);
    }

    public static readonly StyledProperty<string?> HeaderProperty = AvaloniaProperty.Register<
        TileTemplatedControl,
        string?
    >(nameof(Header));

    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly StyledProperty<AsvColorKind> IconColorProperty =
        AvaloniaProperty.Register<TileTemplatedControl, AsvColorKind>(
            nameof(IconColor),
            AsvColorKind.Error
        );

    public AsvColorKind IconColor
    {
        get => GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    public static readonly StyledProperty<TileDensity> DensityProperty = AvaloniaProperty.Register<
        TileTemplatedControl,
        TileDensity
    >(nameof(Density), TileDensity.Regular);

    public TileDensity Density
    {
        get => GetValue(DensityProperty);
        set => SetValue(DensityProperty, value);
    }

    public static readonly StyledProperty<MaterialIconKind?> StatusIconProperty =
        AvaloniaProperty.Register<TileTemplatedControl, MaterialIconKind?>(nameof(StatusIcon));

    public MaterialIconKind? StatusIcon
    {
        get => GetValue(StatusIconProperty);
        set => SetValue(StatusIconProperty, value);
    }

    public static readonly StyledProperty<AsvColorKind> StatusIconColorProperty =
        AvaloniaProperty.Register<TileTemplatedControl, AsvColorKind>(nameof(StatusIconColor));

    public AsvColorKind StatusIconColor
    {
        get => GetValue(StatusIconColorProperty);
        set => SetValue(StatusIconColorProperty, value);
    }

    private void UpdateDensityClasses()
    {
        Classes.Remove(RegularClass);
        Classes.Remove(CompactClass);
        Classes.Remove(InlineClass);

        Classes.Add(
            Density switch
            {
                TileDensity.Compact => CompactClass,
                TileDensity.Inline => InlineClass,
                _ => RegularClass,
            }
        );
    }
}

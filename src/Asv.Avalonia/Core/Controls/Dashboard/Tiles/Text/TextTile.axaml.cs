using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Material.Icons;

namespace Asv.Avalonia;

public class TextTile : TemplatedControl
{
    private const string RegularClass = "regular";
    private const string CompactClass = "compact";
    private const string InlineClass = "inline";

    static TextTile()
    {
        DensityProperty.Changed.AddClassHandler<TextTile>((view, _) => view.UpdateDensityClasses());
    }

    public TextTile()
    {
        UpdateDensityClasses();
    }

    public static readonly StyledProperty<MaterialIconKind> IconProperty =
        AvaloniaProperty.Register<TextTile, MaterialIconKind>(
            nameof(Icon),
            MaterialIconKind.Telecoil
        );

    public MaterialIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string?> ShortHeaderProperty = AvaloniaProperty.Register<
        TextTile,
        string?
    >(nameof(ShortHeader));

    public string? ShortHeader
    {
        get => GetValue(ShortHeaderProperty);
        set => SetValue(ShortHeaderProperty, value);
    }

    public static readonly StyledProperty<string?> HeaderProperty = AvaloniaProperty.Register<
        TextTile,
        string?
    >(nameof(Header));

    public string? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly StyledProperty<AsvColorKind> IconColorProperty =
        AvaloniaProperty.Register<TextTile, AsvColorKind>(nameof(IconColor), AsvColorKind.Error);

    public AsvColorKind IconColor
    {
        get => GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    public static readonly StyledProperty<AsvColorKind> StatusColorProperty =
        AvaloniaProperty.Register<TextTile, AsvColorKind>(
            nameof(StatusColor),
            AsvColorKind.Info3 | AsvColorKind.Blink
        );

    public AsvColorKind StatusColor
    {
        get => GetValue(StatusColorProperty);
        set => SetValue(StatusColorProperty, value);
    }

    public static readonly StyledProperty<string?> StatusTextProperty = AvaloniaProperty.Register<
        TextTile,
        string?
    >(nameof(StatusText));

    public string? StatusText
    {
        get => GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    public static readonly StyledProperty<AsvColorKind> StatusTextColorProperty =
        AvaloniaProperty.Register<TextTile, AsvColorKind>(
            nameof(StatusTextColor),
            AsvColorKind.Unknown
        );

    public AsvColorKind StatusTextColor
    {
        get => GetValue(StatusTextColorProperty);
        set => SetValue(StatusTextColorProperty, value);
    }

    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<
        TextTile,
        string?
    >(nameof(Text));

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<AsvColorKind> TextColorProperty =
        AvaloniaProperty.Register<TextTile, AsvColorKind>(nameof(TextColor), AsvColorKind.Error);

    public AsvColorKind TextColor
    {
        get => GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public static readonly StyledProperty<string?> SuperScriptTextProperty =
        AvaloniaProperty.Register<TextTile, string?>(nameof(SuperScriptText));

    public string? SuperScriptText
    {
        get => GetValue(SuperScriptTextProperty);
        set => SetValue(SuperScriptTextProperty, value);
    }

    public static readonly StyledProperty<AsvColorKind> SuperScriptTextColorProperty =
        AvaloniaProperty.Register<TextTile, AsvColorKind>(
            nameof(SuperScriptTextColor),
            defaultValue: AsvColorKind.Unknown
        );

    public AsvColorKind SuperScriptTextColor
    {
        get => GetValue(SuperScriptTextColorProperty);
        set => SetValue(SuperScriptTextColorProperty, value);
    }

    public static readonly StyledProperty<string?> SubScriptTextProperty =
        AvaloniaProperty.Register<TextTile, string?>(nameof(SubScriptText));

    public string? SubScriptText
    {
        get => GetValue(SubScriptTextProperty);
        set => SetValue(SubScriptTextProperty, value);
    }

    public static readonly StyledProperty<AsvColorKind> SubScriptTextColorProperty =
        AvaloniaProperty.Register<TextTile, AsvColorKind>(
            nameof(SubScriptTextColor),
            defaultValue: AsvColorKind.Unknown
        );

    public AsvColorKind SubScriptTextColor
    {
        get => GetValue(SubScriptTextColorProperty);
        set => SetValue(SubScriptTextColorProperty, value);
    }

    public static readonly StyledProperty<string?> UnitsProperty = AvaloniaProperty.Register<
        TextTile,
        string?
    >(nameof(Units));

    public string? Units
    {
        get => GetValue(UnitsProperty);
        set => SetValue(UnitsProperty, value);
    }

    public static readonly StyledProperty<TileDensity> DensityProperty = AvaloniaProperty.Register<
        TextTile,
        TileDensity
    >(nameof(Density), TileDensity.Regular);

    public TileDensity Density
    {
        get => GetValue(DensityProperty);
        set => SetValue(DensityProperty, value);
    }

    public static readonly StyledProperty<double> ProgressProperty = AvaloniaProperty.Register<
        TextTile,
        double
    >(nameof(Progress), double.NaN);

    public double Progress
    {
        get => GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public static readonly StyledProperty<AsvColorKind> ProgressColorProperty =
        AvaloniaProperty.Register<TextTile, AsvColorKind>(nameof(ProgressColor));

    public AsvColorKind ProgressColor
    {
        get => GetValue(ProgressColorProperty);
        set => SetValue(ProgressColorProperty, value);
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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Material.Icons;

namespace Asv.Avalonia;

public class TextTile : TileTemplatedControl
{
    public TextTile()
    {
        DoubleTapped += OnDoubleTapped;
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

    private void OnDoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (DataContext is ITileViewModel tile)
        {
            tile.Density = GetNextDensity(tile.Density);
        }
        else
        {
            Density = GetNextDensity(Density);
        }

        e.Handled = true;
    }

    private static TileDensity GetNextDensity(TileDensity density)
    {
        return density switch
        {
            TileDensity.Inline => TileDensity.Regular,
            _ => TileDensity.Inline,
        };
    }
}

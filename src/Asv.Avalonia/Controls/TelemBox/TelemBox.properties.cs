using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Material.Icons;

namespace Asv.Avalonia;

public partial class TelemBox
{
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<
        TelemBox,
        string
    >(nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<double> ValueProperty = AvaloniaProperty.Register<
        TelemBox,
        double
    >(nameof(Value));

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly StyledProperty<string> UnitsProperty = AvaloniaProperty.Register<
        TelemBox,
        string
    >(nameof(Units));

    public string Units
    {
        get => GetValue(UnitsProperty);
        set => SetValue(UnitsProperty, value);
    }

    public static readonly StyledProperty<bool> IsBusyProperty = AvaloniaProperty.Register<
        TelemBox,
        bool
    >(nameof(IsBusy));

    public bool IsBusy
    {
        get => GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    public static readonly StyledProperty<MaterialIconKind> IconProperty =
        AvaloniaProperty.Register<TelemBox, MaterialIconKind>(nameof(Icon));

    public MaterialIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<IBrush> MainBrushProperty = AvaloniaProperty.Register<
        TelemBox,
        IBrush
    >(nameof(MainBrush));

    public IBrush MainBrush
    {
        get => GetValue(MainBrushProperty);
        set => SetValue(MainBrushProperty, value);
    }

    public static readonly StyledProperty<IBrush> SecondaryBrushProperty =
        AvaloniaProperty.Register<TelemBox, IBrush>(nameof(SecondaryBrush));

    public IBrush SecondaryBrush
    {
        get => GetValue(SecondaryBrushProperty);
        set => SetValue(SecondaryBrushProperty, value);
    }

    public static readonly StyledProperty<Control> CustomContentProperty =
        AvaloniaProperty.Register<TelemBox, Control>(nameof(CustomContent));

    public Control CustomContent
    {
        get => GetValue(CustomContentProperty);
        set => SetValue(CustomContentProperty, value);
    }
}
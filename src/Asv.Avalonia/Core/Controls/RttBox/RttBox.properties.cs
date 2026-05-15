using Avalonia;
using Avalonia.Controls.Templates;
using Avalonia.Media;
using Avalonia.Metadata;
using Material.Icons;

namespace Asv.Avalonia;

public partial class RttBox
{
    public static readonly DirectProperty<RttBox, MaterialIconKind?> IconProperty =
        AvaloniaProperty.RegisterDirect<RttBox, MaterialIconKind?>(
            nameof(Icon),
            o => o.Icon,
            (o, v) => o.Icon = v
        );

    public MaterialIconKind? Icon
    {
        get;
        set => SetAndRaise(IconProperty, ref field, value);
    }

    public static readonly StyledProperty<IBrush?> IconBrushProperty = AvaloniaProperty.Register<
        RttBox,
        IBrush?
    >(nameof(IconBrush));

    public IBrush? IconBrush
    {
        get => GetValue(IconBrushProperty);
        set => SetValue(IconBrushProperty, value);
    }

    public static readonly DirectProperty<RttBox, string?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<RttBox, string?>(
            nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v
        );

    public string? Header
    {
        get;
        set => SetAndRaise(HeaderProperty, ref field, value);
    }

    public static readonly DirectProperty<RttBox, bool?> IsUpdatedProperty =
        AvaloniaProperty.RegisterDirect<RttBox, bool?>(
            nameof(IsUpdated),
            o => o.IsUpdated,
            (o, v) => o.IsUpdated = v
        );

    public bool? IsUpdated
    {
        get;
        set => SetAndRaise(IsUpdatedProperty, ref field, value);
    }

    public static readonly StyledProperty<bool> IsExpandedProperty = AvaloniaProperty.Register<
        RttBox,
        bool
    >(nameof(IsExpanded), true);

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly StyledProperty<object?> SmallContentProperty = AvaloniaProperty.Register<
        RttBox,
        object?
    >(nameof(SmallContent));

    public static readonly DirectProperty<RttBox, bool> SupportSmallSizeProperty =
        AvaloniaProperty.RegisterDirect<RttBox, bool>(
            nameof(SupportSmallSize),
            o => o.SupportSmallSize,
            (o, v) => o.SupportSmallSize = v
        );

    public bool SupportSmallSize
    {
        get;
        set => SetAndRaise(SupportSmallSizeProperty, ref field, value);
    } = true;

    [DependsOn(nameof(SmallContentTemplate))]
    public object? SmallContent
    {
        get => GetValue(SmallContentProperty);
        set => SetValue(SmallContentProperty, value);
    }

    public static readonly StyledProperty<IDataTemplate?> SmallContentTemplateProperty =
        AvaloniaProperty.Register<RttBox, IDataTemplate?>(nameof(SmallContentTemplate));

    public IDataTemplate? SmallContentTemplate
    {
        get => GetValue(SmallContentTemplateProperty);
        set => SetValue(SmallContentTemplateProperty, value);
    }

    public static readonly StyledProperty<double> HeaderFontSizeProperty =
        AvaloniaProperty.Register<RttBox, double>(nameof(HeaderFontSize));

    public double HeaderFontSize
    {
        get => GetValue(HeaderFontSizeProperty);
        set => SetValue(HeaderFontSizeProperty, value);
    }

    public static readonly DirectProperty<RttBox, string?> SmallHeaderProperty =
        AvaloniaProperty.RegisterDirect<RttBox, string?>(
            nameof(SmallHeader),
            o => o.SmallHeader,
            (o, v) => o.SmallHeader = v
        );

    public string? SmallHeader
    {
        get;
        set => SetAndRaise(SmallHeaderProperty, ref field, value);
    }

    public static readonly DirectProperty<RttBox, bool?> IsNetworkErrorProperty =
        AvaloniaProperty.RegisterDirect<RttBox, bool?>(
            nameof(IsNetworkError),
            o => o.IsNetworkError,
            (o, v) => o.IsNetworkError = v
        );

    public bool? IsNetworkError
    {
        get;
        set => SetAndRaise(IsNetworkErrorProperty, ref field, value);
    }
}

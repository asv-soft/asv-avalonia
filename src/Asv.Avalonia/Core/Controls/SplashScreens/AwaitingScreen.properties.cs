using Avalonia;
using Material.Icons;

namespace Asv.Avalonia;

public partial class AwaitingScreen
{
    public static readonly DirectProperty<AwaitingScreen, string?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<AwaitingScreen, string?>(
            nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v
        );

    public string? Header
    {
        get;
        set => SetAndRaise(HeaderProperty, ref field, value);
    }

    public static readonly DirectProperty<AwaitingScreen, string?> DescriptionProperty =
        AvaloniaProperty.RegisterDirect<AwaitingScreen, string?>(
            nameof(Description),
            o => o.Description,
            (o, v) => o.Description = v
        );

    public string? Description
    {
        get;
        set => SetAndRaise(DescriptionProperty, ref field, value);
    }

    public static readonly DirectProperty<AwaitingScreen, MaterialIconKind> IconProperty =
        AvaloniaProperty.RegisterDirect<AwaitingScreen, MaterialIconKind>(
            nameof(Icon),
            o => o.Icon,
            (o, v) => o.Icon = v
        );

    public MaterialIconKind Icon
    {
        get;
        set => SetAndRaise(IconProperty, ref field, value);
    } = MaterialIconKind.LanPending;
}

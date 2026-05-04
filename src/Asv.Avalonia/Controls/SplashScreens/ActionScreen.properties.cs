using System.Windows.Input;
using Avalonia;
using Material.Icons;

namespace Asv.Avalonia;

public partial class ActionScreen
{
    public static readonly DirectProperty<ActionScreen, string?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<ActionScreen, string?>(
            nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v
        );

    public string? Header
    {
        get;
        set => SetAndRaise(HeaderProperty, ref field, value);
    }

    public static readonly DirectProperty<ActionScreen, string?> DescriptionProperty =
        AvaloniaProperty.RegisterDirect<ActionScreen, string?>(
            nameof(Description),
            o => o.Description,
            (o, v) => o.Description = v
        );

    public string? Description
    {
        get;
        set => SetAndRaise(DescriptionProperty, ref field, value);
    }

    public static readonly DirectProperty<ActionScreen, MaterialIconKind> IconProperty =
        AvaloniaProperty.RegisterDirect<ActionScreen, MaterialIconKind>(
            nameof(Icon),
            o => o.Icon,
            (o, v) => o.Icon = v
        );

    public MaterialIconKind Icon
    {
        get;
        set => SetAndRaise(IconProperty, ref field, value);
    } = MaterialIconKind.Power;

    public static readonly DirectProperty<ActionScreen, ICommand?> CommandProperty =
        AvaloniaProperty.RegisterDirect<ActionScreen, ICommand?>(
            nameof(Command),
            o => o.Command,
            (o, v) => o.Command = v
        );

    public ICommand? Command
    {
        get;
        set => SetAndRaise(CommandProperty, ref field, value);
    }

    public static readonly DirectProperty<ActionScreen, object?> CommandParameterProperty =
        AvaloniaProperty.RegisterDirect<ActionScreen, object?>(
            nameof(CommandParameter),
            o => o.CommandParameter,
            (o, v) => o.CommandParameter = v
        );

    public object? CommandParameter
    {
        get;
        set => SetAndRaise(CommandParameterProperty, ref field, value);
    }

    public static readonly DirectProperty<ActionScreen, bool> IsExecutingProperty =
        AvaloniaProperty.RegisterDirect<ActionScreen, bool>(
            nameof(IsExecuting),
            o => o.IsExecuting,
            (o, v) => o.IsExecuting = v
        );

    public bool IsExecuting
    {
        get;
        set => SetAndRaise(IsExecutingProperty, ref field, value);
    }
}

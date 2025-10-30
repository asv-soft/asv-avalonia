using System.Windows.Input;
using Avalonia;

namespace Asv.Avalonia.Example;

public partial class ActionExampleBlock
{
    public static readonly StyledProperty<string?> ButtonTextProperty = AvaloniaProperty.Register<
        ActionExampleBlock,
        string?
    >(nameof(ButtonText));

    public string? ButtonText
    {
        get => GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    public static readonly StyledProperty<ICommand?> ButtonCommandProperty =
        AvaloniaProperty.Register<ActionExampleBlock, ICommand?>(nameof(ButtonCommand));

    public ICommand? ButtonCommand
    {
        get => GetValue(ButtonCommandProperty);
        set => SetValue(ButtonCommandProperty, value);
    }
}

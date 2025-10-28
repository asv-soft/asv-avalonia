using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;

namespace Asv.Avalonia.Example;

public class DialogBlock : TemplatedControl
{
    public static readonly StyledProperty<object?> ContentProperty = AvaloniaProperty.Register<
        DialogBlock,
        object?
    >(nameof(Content));
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly StyledProperty<ICommand?> ButtonCommandProperty =
        AvaloniaProperty.Register<DialogBlock, ICommand?>(nameof(ButtonCommand));
    public ICommand? ButtonCommand
    {
        get => GetValue(ButtonCommandProperty);
        set => SetValue(ButtonCommandProperty, value);
    }

    public static readonly StyledProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<DialogBlock, ScrollBarVisibility>(
            nameof(VerticalScrollBarVisibility),
            ScrollBarVisibility.Auto
        );
    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => GetValue(VerticalScrollBarVisibilityProperty);
        set => SetValue(VerticalScrollBarVisibilityProperty, value);
    }

    public static readonly StyledProperty<HorizontalAlignment> ContentHorizontalAlignmentProperty =
        AvaloniaProperty.Register<DialogBlock, HorizontalAlignment>(
            nameof(ContentHorizontalAlignment),
            HorizontalAlignment.Stretch
        );
    public HorizontalAlignment ContentHorizontalAlignment
    {
        get => GetValue(ContentHorizontalAlignmentProperty);
        set => SetValue(ContentHorizontalAlignmentProperty, value);
    }

    public static readonly StyledProperty<VerticalAlignment> ContentVerticalAlignmentProperty =
        AvaloniaProperty.Register<DialogBlock, VerticalAlignment>(
            nameof(ContentVerticalAlignment),
            VerticalAlignment.Stretch
        );
    public VerticalAlignment ContentVerticalAlignment
    {
        get => GetValue(ContentVerticalAlignmentProperty);
        set => SetValue(ContentVerticalAlignmentProperty, value);
    }
}

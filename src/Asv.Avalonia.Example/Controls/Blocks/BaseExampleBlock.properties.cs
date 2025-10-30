using Avalonia;

namespace Asv.Avalonia.Example;

public partial class BaseExampleBlock
{
    public static readonly StyledProperty<object> ContentProperty = AvaloniaProperty.Register<
        BaseExampleBlock,
        object
    >(nameof(Content));

    public object Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }
}

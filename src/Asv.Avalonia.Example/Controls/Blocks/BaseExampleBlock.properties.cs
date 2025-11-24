using Avalonia;
using Avalonia.Media;

namespace Asv.Avalonia.Example;

public partial class BaseExampleBlock
{
    public static new readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<PreviewExampleBlock, IBrush?>(nameof(Background));

    public new IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }
}

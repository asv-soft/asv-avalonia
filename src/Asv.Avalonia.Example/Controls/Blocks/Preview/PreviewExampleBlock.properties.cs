using System.Windows.Input;
using Avalonia;

namespace Asv.Avalonia.Example;

public partial class PreviewExampleBlock
{
    public static readonly StyledProperty<object?> PreviewContentProperty =
        AvaloniaProperty.Register<PreviewExampleBlock, object?>(nameof(PreviewContent));

    public object? PreviewContent
    {
        get => GetValue(PreviewContentProperty);
        set => SetValue(PreviewContentProperty, value);
    }

    public static readonly StyledProperty<ICommand?> RestoreCommandProperty =
        AvaloniaProperty.Register<PreviewExampleBlock, ICommand?>(nameof(RestoreCommand));

    public ICommand? RestoreCommand
    {
        get => GetValue(RestoreCommandProperty);
        set => SetValue(RestoreCommandProperty, value);
    }
}

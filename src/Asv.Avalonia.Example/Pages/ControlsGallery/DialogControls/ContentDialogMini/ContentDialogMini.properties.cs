using Avalonia;

namespace Asv.Avalonia.Example;

public partial class ContentDialogMini
{
    public static readonly StyledProperty<ContentDialogResult?> DialogResultProperty =
        AvaloniaProperty.Register<ContentDialogMini, ContentDialogResult?>(
            nameof(DialogResult),
            ContentDialogResult.None
        );

    public ContentDialogResult? DialogResult
    {
        get => GetValue(DialogResultProperty);
        set => SetValue(DialogResultProperty, value);
    }
}

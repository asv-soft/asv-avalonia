using Avalonia;

namespace Asv.Avalonia.Example;

public partial class ContentDialogMini
{
    private const string PrimaryIndicatorElementName = "PART_PrimaryIndicator";
    private const string SecondaryIndicatorElementName = "PART_SecondaryIndicator";
    private const string TextPlaceHolderElementName = "PART_TextPlaceHolder";
    private const string ImagePlaceHolderElementName = "PART_ImagePlaceHolder";

    private const string ActiveStyleClass = "active";

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

    public static readonly StyledProperty<bool> IsImageContentTypeProperty =
        AvaloniaProperty.Register<ContentDialogMini, bool>(nameof(IsImageContentType));

    public bool IsImageContentType
    {
        get => GetValue(IsImageContentTypeProperty);
        set => SetValue(IsImageContentTypeProperty, value);
    }
}

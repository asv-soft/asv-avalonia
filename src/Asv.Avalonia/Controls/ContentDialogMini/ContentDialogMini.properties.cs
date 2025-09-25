using Avalonia;

namespace Asv.Avalonia;

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

    public static readonly StyledProperty<bool> DialogIsImageContentTypeProperty =
        AvaloniaProperty.Register<ContentDialogMini, bool>(nameof(DialogIsImageContentType));

    public bool DialogIsImageContentType
    {
        get => GetValue(DialogIsImageContentTypeProperty);
        set => SetValue(DialogIsImageContentTypeProperty, value);
    }
}

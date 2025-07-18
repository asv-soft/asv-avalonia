using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Templates;

namespace Asv.Avalonia;

[PseudoClasses(PseudoClassesHelper.Hidden, PseudoClassesHelper.Open)]
[PseudoClasses(s_pcPrimary, s_pcSecondary, s_pcClose)]
[PseudoClasses(s_pcFullSize)]
[TemplatePart(s_tpPrimaryButton, typeof(Button))]
[TemplatePart(s_tpSecondaryButton, typeof(Button))]
[TemplatePart(s_tpCloseButton, typeof(Button))]
public partial class ContentDialog
{
    private const string s_tpPrimaryButton = "PrimaryButton";
    private const string s_tpSecondaryButton = "SecondaryButton";
    private const string s_tpCloseButton = "CloseButton";

    private const string s_pcPrimary = ":primary";
    private const string s_pcSecondary = ":secondary";
    private const string s_pcClose = ":close";
    private const string s_pcFullSize = ":fullsize";

    /// <summary>
    /// Defines the <see cref="CloseButtonText"/> property.
    /// </summary>
    public static readonly StyledProperty<string> CloseButtonTextProperty =
        AvaloniaProperty.Register<ContentDialog, string>(nameof(CloseButtonText));

    /// <summary>
    /// Defines the <see cref="DefaultButton"/> property.
    /// </summary>
    public static readonly StyledProperty<ContentDialogButton> DefaultButtonProperty =
        AvaloniaProperty.Register<ContentDialog, ContentDialogButton>(nameof(DefaultButton));

    /// <summary>
    /// Defines the <see cref="IsPrimaryButtonEnabled"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPrimaryButtonEnabledProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsPrimaryButtonEnabled), true);

    /// <summary>
    /// Defines the <see cref="IsSecondaryButtonEnabled"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSecondaryButtonEnabledProperty =
        AvaloniaProperty.Register<ContentDialog, bool>(nameof(IsSecondaryButtonEnabled), true);

    /// <summary>
    /// Defines the <see cref="PrimaryButtonText"/> property.
    /// </summary>
    public static readonly StyledProperty<string> PrimaryButtonTextProperty =
        AvaloniaProperty.Register<ContentDialog, string>(nameof(PrimaryButtonText));

    /// <summary>
    /// Defines the <see cref="SecondaryButtonText"/> property.
    /// </summary>
    public static readonly StyledProperty<string> SecondaryButtonTextProperty =
        AvaloniaProperty.Register<ContentDialog, string>(nameof(SecondaryButtonText));

    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<object> TitleProperty = AvaloniaProperty.Register<
        ContentDialog,
        object
    >(nameof(Title), string.Empty);

    /// <summary>
    /// Defines the <see cref="TitleTemplate"/> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate> TitleTemplateProperty =
        AvaloniaProperty.Register<ContentDialog, IDataTemplate>(nameof(TitleTemplate));

    /// <summary>
    /// Defines the <see cref="FullSizeDesired"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> FullSizeDesiredProperty = AvaloniaProperty.Register<
        ContentDialog,
        bool
    >(nameof(FullSizeDesired));

    /// <summary>
    /// Gets or sets the text to display on the close button.
    /// </summary>
    public string CloseButtonText
    {
        get => GetValue(CloseButtonTextProperty);
        set => SetValue(CloseButtonTextProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates which button on the dialog is the default action.
    /// </summary>
    public ContentDialogButton DefaultButton
    {
        get => GetValue(DefaultButtonProperty);
        set => SetValue(DefaultButtonProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the dialog's primary button is enabled.
    /// </summary>
    public bool IsPrimaryButtonEnabled
    {
        get => GetValue(IsPrimaryButtonEnabledProperty);
        set => SetValue(IsPrimaryButtonEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the dialog's secondary button is enabled.
    /// </summary>
    public bool IsSecondaryButtonEnabled
    {
        get => GetValue(IsSecondaryButtonEnabledProperty);
        set => SetValue(IsSecondaryButtonEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the text to display on the primary button.
    /// </summary>
    public string PrimaryButtonText
    {
        get => GetValue(PrimaryButtonTextProperty);
        set => SetValue(PrimaryButtonTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the text to be displayed on the secondary button.
    /// </summary>
    public string SecondaryButtonText
    {
        get => GetValue(SecondaryButtonTextProperty);
        set => SetValue(SecondaryButtonTextProperty, value);
    }

    /// <summary>
    /// Gets or sets the title of the dialog.
    /// </summary>
    public object Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the title template.
    /// </summary>
    public IDataTemplate TitleTemplate
    {
        get => GetValue(TitleTemplateProperty);
        set => SetValue(TitleTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the Dialog should show full screen
    /// On WinUI3, at least desktop, this just show the dialog at
    /// the maximum size of a Content Dialog.
    /// </summary>
    public bool FullSizeDesired
    {
        get => GetValue(FullSizeDesiredProperty);
        set => SetValue(FullSizeDesiredProperty, value);
    }

    /// <summary>
    /// Occurs before the dialog is opened.
    /// </summary>
    public event TypedEventHandler<ContentDialog, EventArgs>? Opening;

    /// <summary>
    /// Occurs after the dialog is opened.
    /// </summary>
    public event TypedEventHandler<ContentDialog, EventArgs>? Opened;

    /// <summary>
    /// Occurs after the dialog starts to close, but before it is closed and before the Closed event occurs.
    /// </summary>
    public event TypedEventHandler<ContentDialog, ContentDialogClosingEventArgs>? Closing;

    /// <summary>
    /// Occurs after the dialog is closed.
    /// </summary>
    public event TypedEventHandler<ContentDialog, ContentDialogClosedEventArgs>? Closed;

    /// <summary>
    /// Occurs after the primary button has been tapped.
    /// </summary>
    public event TypedEventHandler<
        ContentDialog,
        ContentDialogButtonClickEventArgs
    >? PrimaryButtonClick;

    /// <summary>
    /// Occurs after the secondary button has been tapped.
    /// </summary>
    public event TypedEventHandler<
        ContentDialog,
        ContentDialogButtonClickEventArgs
    >? SecondaryButtonClick;

    /// <summary>
    /// Occurs after the close button has been tapped.
    /// </summary>
    public event TypedEventHandler<
        ContentDialog,
        ContentDialogButtonClickEventArgs
    >? CloseButtonClick;
}

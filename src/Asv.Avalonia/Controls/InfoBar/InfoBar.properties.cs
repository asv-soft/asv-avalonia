﻿using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Asv.Avalonia;

/// <summary>
/// An InfoBar is an inline notification for essential app-wide messages.
/// The InfoBar will take up space in a layout and will not cover up other content
/// or float on top of it. It supports rich content (including titles, messages, icons, and buttons)
/// and can be configured to be user-dismissible or persistent.
/// </summary>
[PseudoClasses(PseudoClassesHelper.Hidden, PseudoClassesHelper.CloseHidden)]
[PseudoClasses(
    PseudoClassesHelper.Success,
    PseudoClassesHelper.Warning,
    PseudoClassesHelper.Error,
    PseudoClassesHelper.Informational
)]
[PseudoClasses(PseudoClassesHelper.Icon, PseudoClassesHelper.StandardIcon)]
[PseudoClasses(PseudoClassesHelper.ForegroundSet)]
[TemplatePart(s_tpCloseButton, typeof(Button))]
public partial class InfoBar : ContentControl
{
    /// <summary>
    /// Defines the <see cref="IsOpen"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<
        InfoBar,
        bool
    >(nameof(IsOpen));

    /// <summary>
    /// Defines the <see cref="Title"/> property.
    /// </summary>
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<
        InfoBar,
        string
    >(nameof(Title));

    /// <summary>
    /// Defines the <see cref="Message"/> property.
    /// </summary>
    public static readonly StyledProperty<string> MessageProperty = AvaloniaProperty.Register<
        InfoBar,
        string
    >(nameof(Message));

    /// <summary>
    /// Defines the <see cref="Severity"/> property.
    /// </summary>
    public static readonly StyledProperty<InfoBarSeverity> SeverityProperty =
        AvaloniaProperty.Register<InfoBar, InfoBarSeverity>(nameof(Severity));

    /// <summary>
    /// Defines the <see cref="IsIconVisible"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsIconVisibleProperty = AvaloniaProperty.Register<
        InfoBar,
        bool
    >(nameof(IsIconVisible), true);

    /// <summary>
    /// Defines the <see cref="IsClosable"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsClosableProperty = AvaloniaProperty.Register<
        InfoBar,
        bool
    >(nameof(IsClosable), true);

    /// <summary>
    /// Defines the <see cref="CloseButtonCommand"/> property.
    /// </summary>
    public static readonly StyledProperty<ICommand> CloseButtonCommandProperty =
        AvaloniaProperty.Register<InfoBar, ICommand>(nameof(CloseButtonCommand));

    /// <summary>
    /// Defines the <see cref="CloseButtonCommandParameter"/> property.
    /// </summary>
    public static readonly StyledProperty<object> CloseButtonCommandParameterProperty =
        AvaloniaProperty.Register<InfoBar, object>(nameof(CloseButtonCommandParameter));

    /// <summary>
    /// Defines the <see cref="ActionButton"/> property.
    /// </summary>
    public static readonly StyledProperty<Control> ActionButtonProperty = AvaloniaProperty.Register<
        InfoBar,
        Control
    >(nameof(ActionButton));

    /// <summary>
    /// Gets or sets a value indicating whether the InfoBar is opened.
    /// </summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets the title of the InfoBar.
    /// </summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Gets or sets the message of the InfoBar.
    /// </summary>
    public string Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>
    /// Gets or sets the type of the InfoBar to apply consistent status color, icon,
    /// and assistive technology settings dependent on the criticality of the notification.
    /// </summary>
    public InfoBarSeverity Severity
    {
        get => GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the icon is visible in the InfoBar.
    /// </summary>
    public bool IsIconVisible
    {
        get => GetValue(IsIconVisibleProperty);
        set => SetValue(IsIconVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the user can close the InfoBar.
    /// </summary>
    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>
    /// Gets or sets the command to invoke when the close button is clicked in the InfoBar.
    /// </summary>
    public ICommand CloseButtonCommand
    {
        get => GetValue(CloseButtonCommandProperty);
        set => SetValue(CloseButtonCommandProperty, value);
    }

    /// <summary>
    /// Gets or sets the parameter to pass to the command for the close button in the InfoBar.
    /// </summary>
    public object CloseButtonCommandParameter
    {
        get => GetValue(CloseButtonCommandParameterProperty);
        set => SetValue(CloseButtonCommandParameterProperty, value);
    }

    /// <summary>
    /// Gets or sets the action button of the InfoBar.
    /// </summary>
    public Control ActionButton
    {
        get => GetValue(ActionButtonProperty);
        set => SetValue(ActionButtonProperty, value);
    }

    /// <summary>
    /// Occurs after the close button is clicked in the InfoBar.
    /// </summary>
    public event TypedEventHandler<InfoBar, EventArgs>? CloseButtonClick;

    /// <summary>
    /// Occurs just before the InfoBar begins to close.
    /// </summary>
    public event TypedEventHandler<InfoBar, InfoBarClosingEventArgs>? Closing;

    /// <summary>
    /// Occurs after the InfoBar is closed.
    /// </summary>
    public event TypedEventHandler<InfoBar, InfoBarClosedEventArgs>? Closed;

    private const string SR_InfoBarCloseButtonTooltip = "InfoBarCloseButtonTooltip";

    private const string s_tpCloseButton = "CloseButton";
}

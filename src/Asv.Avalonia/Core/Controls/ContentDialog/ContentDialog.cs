using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Asv.Avalonia;

/// <summary>
/// Presents an asynchronous dialog to the user.
/// <remarks>
/// Code was taken from: https://github.com/amwx/FluentAvalonia.
/// </remarks>
/// </summary>
public partial class ContentDialog : ContentControl, ICustomKeyboardNavigation
{
    private IInputElement? _lastFocus;
    private Control? _originalHost;
    private int _originalHostIndex;
    private DialogHost? _host;
    private ContentDialogResult _result;
    private TaskCompletionSource<ContentDialogResult> _tcs;
    private Button? _primaryButton;
    private Button? _secondaryButton;
    private Button? _closeButton;
    private bool _hasDeferralActive;
    private Visual? _hotkeyDownVisual;

    private readonly INavigationService _navigation;

    public ContentDialog()
        : this(NullDialogViewModel.Instance, DesignTime.Navigation)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public ContentDialog(DialogViewModelBase content, INavigationService navigationService)
    {
        _navigation = navigationService;
        Content = content;
        PseudoClasses.Add(PseudoClassesHelper.Hidden);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_primaryButton != null)
        {
            _primaryButton.Click -= OnButtonClick;
        }

        if (_secondaryButton != null)
        {
            _secondaryButton.Click -= OnButtonClick;
        }

        if (_closeButton != null)
        {
            _closeButton.Click -= OnButtonClick;
        }

        base.OnApplyTemplate(e);

        _primaryButton = e.NameScope.Get<Button>(s_tpPrimaryButton);
        _primaryButton.Click += OnButtonClick;
        _secondaryButton = e.NameScope.Get<Button>(s_tpSecondaryButton);
        _secondaryButton.Click += OnButtonClick;
        _closeButton = e.NameScope.Get<Button>(s_tpCloseButton);
        _closeButton.Click += OnButtonClick;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == FullSizeDesiredProperty)
        {
            OnFullSizedDesiredChanged(change);
        }

        if (change.Property == IsPrimaryButtonEnabledProperty)
        {
            if (_primaryButton != null)
            {
                _primaryButton.IsEnabled = IsPrimaryButtonEnabled;
            }
        }
    }

    protected override bool RegisterContentPresenter(ContentPresenter presenter)
    {
        return presenter.Name == "Content" || base.RegisterContentPresenter(presenter);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // See OnKeyUp for reasoning
        if (e is { Handled: false, Key: Key.Enter or Key.Escape, Source: Visual visual })
        {
            _hotkeyDownVisual = visual;
        }
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        if (e.Handled)
        {
            base.OnKeyUp(e);
            return;
        }

        // HACK: If a default button is set, and the content dialog is opened by enter or escape key press on a button
        // the KeyUp event is raised on the focused item within the dialog, instead of the original button
        // (Avalonia related issue #9626) and will immediately close the dialog
        // We store the source of the key down and if it doesn't match, or hasn't been set yet, we ignore
        // this key up event so we don't inadvertently close the dialog
        var visualSrc = e.Source as Visual;
        if (e.Key is Key.Enter or Key.Escape && _hotkeyDownVisual != visualSrc)
        {
            base.OnKeyUp(e);
            return;
        }

        switch (e.Key)
        {
            case Key.Escape:
                HideCore();
                e.Handled = true;
                break;

            case Key.Enter:
                var defButton = DefaultButton;

                // v2 - Only handle 'Enter' if the default button is set
                //      Otherwise, we'll let the event go as normal - if focus is currently
                //      on a button, 'Enter' should invoke that button
                if (defButton != ContentDialogButton.None)
                {
                    switch (defButton)
                    {
                        case ContentDialogButton.Primary:
                            OnButtonClick(_primaryButton, null);
                            break;

                        case ContentDialogButton.Secondary:
                            OnButtonClick(_secondaryButton, null);
                            break;

                        case ContentDialogButton.Close:
                            OnButtonClick(_closeButton, null);
                            break;
                    }

                    e.Handled = true;
                }

                break;
        }

        base.OnKeyUp(e);
    }

    /// <summary>
    /// Begins an asynchronous operation to show the dialog.
    /// </summary>
    /// <returns>Returns <inheritdoc cref="ContentDialogResult"/> that shows which dialog button was pressed.</returns>
    public Task<ContentDialogResult> ShowAsync() => ShowAsyncCoreForTopLevel(null);

    /// <summary>
    /// Begins an asynchronous operation to show the dialog using the specified window.
    /// </summary>
    /// <param name="window">.</param>
    /// <returns>Returns <inheritdoc cref="ContentDialogResult"/> that shows which dialog button was pressed.</returns>
    public Task<ContentDialogResult> ShowAsync(Window window) => ShowAsyncCoreForTopLevel(window);

    /// <summary>
    /// Begins an asynchronous operation to show the dialog using the specified top level.
    /// </summary>
    /// <remarks>
    /// Use this when an ApplicationLifetime is unavailable (such as in headless unit tests).
    /// </remarks>
    /// <param name="topLevel">.</param>
    /// <returns>Returns <inheritdoc cref="ContentDialogResult"/> that shows which dialog button was pressed.</returns>
    public Task<ContentDialogResult> ShowAsync(TopLevel topLevel) =>
        ShowAsyncCoreForTopLevel(topLevel);

    private async Task<ContentDialogResult> ShowAsyncCoreForTopLevel(TopLevel? topLevel)
    {
        if (Content is not DialogViewModelBase)
        {
            throw new Exception($"Content type is not {nameof(DialogViewModelBase)}");
        }

        _tcs = new TaskCompletionSource<ContentDialogResult>();

        OnOpening();

        if (Parent != null)
        {
            _originalHost = (Control)Parent;
            switch (_originalHost)
            {
                case Panel p:
                    _originalHostIndex = p.Children.IndexOf(this);
                    p.Children.Remove(this);
                    break;
                case Decorator d:
                    d.Child = null;
                    break;
                case ContentControl cc:
                    cc.Content = null;
                    break;
                case ContentPresenter cp:
                    cp.Content = null;
                    break;
            }
        }

        _host ??= new DialogHost();
        _host.Content = this;

        OverlayLayer? ol;

        if (topLevel != null)
        {
            ol = OverlayLayer.GetOverlayLayer(topLevel);
        }
        else
        {
            if (
                Application.Current?.ApplicationLifetime
                is IClassicDesktopStyleApplicationLifetime applicationLifetime
            )
            {
                var windows = applicationLifetime.Windows;
                foreach (var window in windows)
                {
                    if (!window.IsActive)
                    {
                        continue;
                    }

                    topLevel = window;
                    break;
                }

                topLevel ??=
                    applicationLifetime.MainWindow
                    ?? throw new NotSupportedException(
                        "No TopLevel root found to parent ContentDialog"
                    );

                ol = OverlayLayer.GetOverlayLayer(topLevel);
            }
            else if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime sl)
            {
                topLevel =
                    TopLevel.GetTopLevel(sl.MainView)
                    ?? throw new NotSupportedException(
                        "No TopLevel root found to parent ContentDialog"
                    );

                ol = OverlayLayer.GetOverlayLayer(topLevel);
            }
            else
            {
                throw new InvalidOperationException(
                    "No TopLevel found for ContentDialog and no ApplicationLifetime is set. "
                        + "Please either supply a valid ApplicationLifetime or TopLevel to ShowAsync()"
                );
            }
        }

        if (ol == null)
        {
            throw new InvalidOperationException("Unable to find OverlayLayer from given TopLevel");
        }

        _lastFocus = topLevel.FocusManager?.GetFocusedElement();
        TrySetDataContext();

        // TrySetDataContext();
        ol.Children.Add(_host);

        // Make the dialog visible
        IsVisible = true;
        PseudoClasses.Set(PseudoClassesHelper.Hidden, false);
        PseudoClasses.Set(PseudoClassesHelper.Open, true);

        // Delay further initializing until after the dialog has loaded. We sub here and unsub in DialogLoaded
        // because ContentDialog can be declared in Xaml prior to showing, and we don't want Loaded to trigger
        // initialization if we're not actually showing the dialog
        Loaded += DialogLoaded;

        return await _tcs.Task;
    }

    private bool TrySetDataContext()
    {
        if (Content is not IRoutable routableContent)
        {
            throw new Exception($"{nameof(Content)} is not {typeof(IRoutable)}");
        }

        if (_lastFocus is not InputElement last)
        {
            return false;
        }

        var attemptsCount = 0;
        var routable = last.DataContext as IRoutable;
        var parent = last.Parent;
        while (routable is null)
        {
            if (parent is null)
            {
                return false;
            }

            if (attemptsCount > 100)
            {
                return false;
            }

            routable = parent.DataContext as IRoutable;
            parent = parent.Parent;
            attemptsCount++;
        }

        routableContent.Parent = routable;
        DataContext = routableContent;
        return true;
    }

    /// <summary>
    /// Closes the current <see cref="ContentDialog"/> without a result (<see cref="ContentDialogResult"/>.<see cref="ContentDialogResult.None"/>).
    /// </summary>
    public void Hide() => Hide(ContentDialogResult.None);

    /// <summary>
    /// Closes the current <see cref="ContentDialog"/> with the given <see cref="ContentDialogResult"/>.
    /// <para>ddd.</para>
    /// </summary>
    /// <param name="dialogResult">The <see cref="ContentDialogResult"/> to return.</param>
    public void Hide(ContentDialogResult dialogResult)
    {
        _result = dialogResult;
        HideCore();
    }

    /// <summary>
    /// Called when the primary button is invoked.
    /// </summary>
    /// <param name="args">.</param>
    protected virtual void OnPrimaryButtonClick(ContentDialogButtonClickEventArgs args)
    {
        PrimaryButtonClick?.Invoke(this, args);
    }

    /// <summary>
    /// Called when the secondary button is invoked.
    /// </summary>
    /// <param name="args">.</param>
    protected virtual void OnSecondaryButtonClick(ContentDialogButtonClickEventArgs args)
    {
        SecondaryButtonClick?.Invoke(this, args);
    }

    /// <summary>
    /// Called when the close button is invoked.
    /// </summary>
    /// <param name="args">.</param>
    protected virtual void OnCloseButtonClick(ContentDialogButtonClickEventArgs args)
    {
        CloseButtonClick?.Invoke(this, args);
    }

    /// <summary>
    /// Called when the ContentDialog is requested to be opened.
    /// </summary>
    protected virtual void OnOpening()
    {
        Opening?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called after the ContentDialog is initialized but just before it is presented on screen.
    /// </summary>
    protected virtual void OnOpened()
    {
        Opened?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the ContentDialog has been requested to close, but before it actually closes.
    /// </summary>
    /// <param name="args">.</param>
    protected virtual void OnClosing(ContentDialogClosingEventArgs args)
    {
        Closing?.Invoke(this, args);
    }

    /// <summary>
    /// Called when the ContentDialog has been closed and removed from the tree.
    /// </summary>
    /// <param name="args">.</param>
    protected virtual void OnClosed(ContentDialogClosedEventArgs args)
    {
        Closed?.Invoke(this, args);
    }

    private void HideCore()
    {
        if (_hasDeferralActive)
        {
            return;
        }

        var args = new ContentDialogClosingEventArgs(_result);

        var deferral = new Deferral(async () =>
        {
            Dispatcher.UIThread.VerifyAccess();
            _hasDeferralActive = false;

            if (!args.Cancel)
            {
                await FinalCloseDialog();
            }
        });

        args.SetDeferral(deferral);
        _hasDeferralActive = true;

        args.IncrementDeferralCount();
        OnClosing(args);
        args.DecrementDeferralCount();
    }

    // This is the exit point for the ContentDialog
    // This method MUST be called to finalize everything
    private async ValueTask FinalCloseDialog()
    {
        // Prevent interaction when closing...double/multiple clicking on the buttons to close
        // the dialog was calling this multiple times, which would cause the OverlayLayer check
        // below to fail (as this would be removed from the tree). This is a simple workaround
        // to make sure we don't error out
        IsHitTestVisible = false;

        // For a better experience when animating closed, we need to make sure the
        // focus adorner is not showing (if using keyboard) otherwise that will hang
        // around and not fade out, and it just looks weird. So focus this to force the
        // adorner to hide, then continue forward.
        Focus();

        PseudoClasses.Set(PseudoClassesHelper.Hidden, true);
        PseudoClasses.Set(PseudoClassesHelper.Open, false);

        // Let the close animation finish (now 0.167s in new WinUI update...)
        // We'll wait just a touch longer to be sure
        await Task.Delay(200);

        OnClosed(new ContentDialogClosedEventArgs(_result));

        if (_lastFocus != null)
        {
            _lastFocus.Focus();
            _lastFocus = null;
        }

        if (_host is null)
        {
            throw new Exception($"{nameof(_host)} is null");
        }

        var ol = OverlayLayer.GetOverlayLayer(_host);

        // If OverlayLayer isn't found here, this may be a reentrant call (hit ESC multiple times quickly, etc)
        // Don't fail, and return. If this isn't reentrant, there's bigger issues...
        if (ol == null)
        {
            return;
        }

        ol.Children.Remove(_host);

        _host.Content = null;

        if (_originalHost != null)
        {
            if (_originalHost is Panel p)
            {
                p.Children.Insert(_originalHostIndex, this);
            }
            else if (_originalHost is Decorator d)
            {
                d.Child = this;
            }
            else if (_originalHost is ContentControl cc)
            {
                cc.Content = this;
            }
            else if (_originalHost is ContentPresenter cp)
            {
                cp.Content = this;
            }
        }

        _hotkeyDownVisual = null;

        // Move focus to the previous IRoutable
        if (DataContext is IRoutable routable)
        {
            if (routable.Parent is null)
            {
                return;
            }

            _navigation.ForceFocus(routable.Parent);
        }

        _tcs.TrySetResult(_result);
    }

    private void OnButtonClick(object? sender, RoutedEventArgs? e)
    {
        // v2 - No longer disabling the dialog during a deferral so we need to make sure that if
        //      multiple requests to close come in, we don't handle them
        if (_hasDeferralActive)
        {
            return;
        }

        var args = new ContentDialogButtonClickEventArgs();

        var deferral = new Deferral(() =>
        {
            Dispatcher.UIThread.VerifyAccess();
            _hasDeferralActive = false;

            if (args.Cancel)
            {
                return ValueTask.CompletedTask;
            }

            if (Equals(sender, _primaryButton))
            {
                _result = ContentDialogResult.Primary;
            }
            else if (Equals(sender, _secondaryButton))
            {
                _result = ContentDialogResult.Secondary;
            }
            else if (Equals(sender, _closeButton))
            {
                _result = ContentDialogResult.None;
            }

            HideCore();

            return ValueTask.CompletedTask;
        });

        args.SetDeferral(deferral);
        _hasDeferralActive = true;

        args.IncrementDeferralCount();
        if (Equals(sender, _primaryButton))
        {
            OnPrimaryButtonClick(args);
        }
        else if (Equals(sender, _secondaryButton))
        {
            OnSecondaryButtonClick(args);
        }
        else if (Equals(sender, _closeButton))
        {
            OnCloseButtonClick(args);
        }

        args.DecrementDeferralCount();
    }

    private void OnFullSizedDesiredChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is bool newVal)
        {
            PseudoClasses.Set(s_pcFullSize, newVal);
        }
    }

    /// <summary>
    /// Unused. TODO: Find out how to get rid of it in our custom version.
    /// </summary>
    /// <param name="element">.</param>
    /// <param name="direction">..</param>
    /// <returns>...</returns>
    public (bool handled, IInputElement? next) GetNext(
        IInputElement element,
        NavigationDirection direction
    )
    {
        var children = this.GetVisualDescendants()
            .OfType<IInputElement>()
            .Where(x =>
                KeyboardNavigation.GetIsTabStop((InputElement)x)
                && x.Focusable
                && x.IsEffectivelyVisible
                && IsEffectivelyEnabled
            )
            .ToList();

        if (children.Count == 0)
        {
            return (false, null);
        }

        var current = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement();
        if (current == null)
        {
            return (false, null);
        }

        if (direction == NavigationDirection.Next)
        {
            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] != current)
                {
                    continue;
                }

                if (i == children.Count - 1)
                {
                    return (true, children[0]);
                }

                return (true, children[i + 1]);
            }
        }
        else if (direction == NavigationDirection.Previous)
        {
            for (int i = children.Count - 1; i >= 0; i--)
            {
                if (children[i] != current)
                {
                    continue;
                }

                if (i == 0)
                {
                    return (true, children[^1]);
                }

                return (true, children[i - 1]);
            }
        }

        return (false, null);
    }

    internal void SetupDialog()
    {
        if (_primaryButton == null)
        {
            throw new InvalidOperationException(
                "Attempted to setup ContentDialog but the template has not been applied yet."
            );
        }

        PseudoClasses.Set(s_pcPrimary, !string.IsNullOrEmpty(PrimaryButtonText));
        PseudoClasses.Set(s_pcSecondary, !string.IsNullOrEmpty(SecondaryButtonText));
        PseudoClasses.Set(s_pcClose, !string.IsNullOrEmpty(CloseButtonText));

        switch (DefaultButton)
        {
            case ContentDialogButton.Primary:
                if (!_primaryButton.IsVisible)
                {
                    break;
                }

                _primaryButton.Classes.Add(PseudoClassesHelper.Accent);
                _secondaryButton?.Classes.Remove(PseudoClassesHelper.Accent);
                _closeButton?.Classes.Remove(PseudoClassesHelper.Accent);

                _primaryButton.Focus();
                break;

            case ContentDialogButton.Secondary:
                if (!_secondaryButton?.IsVisible ?? false)
                {
                    break;
                }

                _secondaryButton?.Classes.Add(PseudoClassesHelper.Accent);
                _primaryButton.Classes.Remove(PseudoClassesHelper.Accent);
                _closeButton?.Classes.Remove(PseudoClassesHelper.Accent);

                _secondaryButton?.Focus();
                break;

            case ContentDialogButton.Close:
                if (!_closeButton?.IsVisible ?? false)
                {
                    break;
                }

                _closeButton?.Classes.Add(PseudoClassesHelper.Accent);
                _primaryButton.Classes.Remove(PseudoClassesHelper.Accent);
                _secondaryButton?.Classes.Remove(PseudoClassesHelper.Accent);
                _closeButton?.Focus();

                break;

            default:
                _closeButton?.Classes.Remove(PseudoClassesHelper.Accent);
                _primaryButton.Classes.Remove(PseudoClassesHelper.Accent);
                _secondaryButton?.Classes.Remove(PseudoClassesHelper.Accent);

                // If no default button is set, try to find a suitable first focus item. If none exist, focus the
                // ContentDialog itself to pull focus away from the main visual tree so weird things don't happen
                // The latter shouldn't happen in 99% of cases as either something in the user content will be able
                // to take focus OR there should always be at least one button which can take focus
#pragma warning disable CS0618 // Type or member is obsolete // TODO: Copy fix for this from FA when they'll come up with it
                var next =
                    KeyboardNavigationHandler.GetNext(this, NavigationDirection.Next) ?? this;
#pragma warning restore CS0618 // Type or member is obsolete
                next.Focus();

                break;
        }
    }

    private void DialogLoaded(object? sender, RoutedEventArgs args)
    {
        Loaded -= DialogLoaded;

        // Run setup, this will set up the buttons and attempt to set initial focus into the dialog
        SetupDialog();

        // Now force a new layout pass so everything in SetupDialog takes effect
        UpdateLayout();

        // Now that we've fully initialized here, raise the Opened event
        OnOpened();
    }
}

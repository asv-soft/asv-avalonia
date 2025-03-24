using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace Asv.Avalonia;

/// <summary>
/// Presents a bar with information to the user.
/// <remarks>
/// Code was taken from: https://github.com/amwx/FluentAvalonia.
/// </remarks>
/// </summary>
public partial class InfoBar : ContentControl
{
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _appliedTemplate = false;
        if (_closeButton != null)
        {
            _closeButton.Click -= OnCloseButtonClick;
        }

        base.OnApplyTemplate(e);

        _closeButton = e.NameScope.Find<Button>(s_tpCloseButton);
        if (_closeButton != null)
        {
            _closeButton.Click += OnCloseButtonClick;

            ToolTip.SetTip(_closeButton, SR_InfoBarCloseButtonTooltip);
        }

        _appliedTemplate = true;

        UpdateVisibility(_notifyOpen, true);
        _notifyOpen = false;

        UpdateSeverity();
        UpdateIconVisibility();
        UpdateCloseButton();
        UpdateForeground();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsOpenProperty)
        {
            if (change.GetNewValue<bool>())
            {
                _lastCloseReason = InfoBarCloseReason.Programmatic;
                UpdateVisibility();
            }
            else
            {
                RaiseClosingEvent();
            }
        }
        else if (change.Property == SeverityProperty)
        {
            UpdateSeverity();
        }
        else if (change.Property == IsIconVisibleProperty)
        {
            UpdateIconVisibility();
        }
        else if (change.Property == IsClosableProperty)
        {
            UpdateCloseButton();
        }
        else if (change.Property == TextElement.ForegroundProperty)
        {
            UpdateForeground();
        }
    }

    protected override bool RegisterContentPresenter(ContentPresenter presenter)
    {
        if (presenter.Name == "ContentPresenter")
        {
            return true;
        }

        return base.RegisterContentPresenter(presenter);
    }

    private void OnCloseButtonClick(object? sender, RoutedEventArgs e)
    {
        CloseButtonClick?.Invoke(this, EventArgs.Empty);
        _lastCloseReason = InfoBarCloseReason.CloseButton;
        IsOpen = false;
    }

    private void RaiseClosingEvent()
    {
        var args = new InfoBarClosingEventArgs(_lastCloseReason);

        Closing?.Invoke(this, args);

        if (!args.Cancel)
        {
            UpdateVisibility();
            RaiseClosedEvent();
        }
        else
        {
            // The developer has changed the Cancel property to true,
            // so we need to revert the IsOpen property to true.
            IsOpen = true;
        }
    }

    private void RaiseClosedEvent()
    {
        var args = new InfoBarClosedEventArgs(_lastCloseReason);
        Closed?.Invoke(this, args);
    }

    private void UpdateVisibility(bool notify = true, bool force = true)
    {
        if (!_appliedTemplate)
        {
            _notifyOpen = true;
        }
        else
        {
            if (force || IsOpen != _isVisible)
            {
                if (IsOpen)
                {
                    _isVisible = true;
                    PseudoClasses.Set(PseudoClassesHelper.Hidden, false);
                }
                else
                {
                    _isVisible = false;
                    PseudoClasses.Set(PseudoClassesHelper.Hidden, true);
                }
            }
        }
    }

    private void UpdateSeverity()
    {
        if (!_appliedTemplate)
        {
            return; // Template not applied yet
        }

        switch (Severity)
        {
            case InfoBarSeverity.Success:
                PseudoClasses.Set(PseudoClassesHelper.Success, true);
                PseudoClasses.Set(PseudoClassesHelper.Warning, false);
                PseudoClasses.Set(PseudoClassesHelper.Error, false);
                PseudoClasses.Set(PseudoClassesHelper.Informational, false);
                break;

            case InfoBarSeverity.Warning:
                PseudoClasses.Set(PseudoClassesHelper.Success, false);
                PseudoClasses.Set(PseudoClassesHelper.Warning, true);
                PseudoClasses.Set(PseudoClassesHelper.Error, false);
                PseudoClasses.Set(PseudoClassesHelper.Informational, false);
                break;

            case InfoBarSeverity.Error:
                PseudoClasses.Set(PseudoClassesHelper.Success, false);
                PseudoClasses.Set(PseudoClassesHelper.Warning, false);
                PseudoClasses.Set(PseudoClassesHelper.Error, true);
                PseudoClasses.Set(PseudoClassesHelper.Informational, false);
                break;

            default: // default to informational
                PseudoClasses.Set(PseudoClassesHelper.Success, false);
                PseudoClasses.Set(PseudoClassesHelper.Warning, false);
                PseudoClasses.Set(PseudoClassesHelper.Error, false);
                PseudoClasses.Set(PseudoClassesHelper.Informational, true);
                break;
        }
    }

    /// <summary>
    /// No use.
    /// </summary>
    private void UpdateIcon() { }

    private void UpdateIconVisibility()
    {
        if (!IsIconVisible)
        {
            PseudoClasses.Set(PseudoClassesHelper.Icon, false);
            PseudoClasses.Set(PseudoClassesHelper.StandardIcon, false);
        }
        else
        {
            PseudoClasses.Set(PseudoClassesHelper.Icon, true);
            PseudoClasses.Set(PseudoClassesHelper.StandardIcon, true);
        }
    }

    private void UpdateCloseButton()
    {
        PseudoClasses.Set(PseudoClassesHelper.CloseHidden, !IsClosable);
    }

    private void UpdateForeground()
    {
        PseudoClasses.Set(
            PseudoClassesHelper.ForegroundSet,
            !Equals(GetValue(TextElement.ForegroundProperty), AvaloniaProperty.UnsetValue)
        );
    }

    private Button? _closeButton;

    private bool _appliedTemplate;
    private bool _notifyOpen;
    private bool _isVisible;

    private InfoBarCloseReason _lastCloseReason;
}

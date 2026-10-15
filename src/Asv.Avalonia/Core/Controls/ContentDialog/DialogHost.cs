using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using R3;

namespace Asv.Avalonia;

/// <summary>
/// Special control to host a <see cref="ContentDialog"/>.
/// </summary>
public class DialogHost : ContentControl
{
    public DialogHost()
    {
        Background = null;
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;
    }

    protected override Type StyleKeyOverride => typeof(OverlayPopupHost);

    protected override Size MeasureOverride(Size availableSize)
    {
        var hostSize = VisualRoot switch
        {
            TopLevel topLevel => topLevel.ClientSize,
            Control control => control.Bounds.Size,
            _ => default,
        };

        // The overlay can measure with infinite space; expanded dialogs need the window bounds.
        var measureSize = Content is ContentDialog { SizeMode: not DialogSizeMode.Compact }
            ? hostSize
            : availableSize;
        _ = base.MeasureOverride(measureSize);
        return hostSize;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (e.RootVisual is Control wb)
        {
            // OverlayLayer is a Canvas, so we won't get a signal to resize if the window
            // bounds change. Subscribe to force update
            _rootBoundsWatcher = wb.GetObservable(BoundsProperty)
                .ToObservable()
                .Subscribe(_ => OnRootBoundsChanged());
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _rootBoundsWatcher?.Dispose();
        _rootBoundsWatcher = null;
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        e.Handled = true;
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        e.Handled = true;
    }

    private void OnRootBoundsChanged()
    {
        InvalidateMeasure();
    }

    private IDisposable? _rootBoundsWatcher;
}

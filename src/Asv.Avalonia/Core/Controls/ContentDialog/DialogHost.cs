using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;

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
        _ = base.MeasureOverride(availableSize);

        if (VisualRoot is TopLevel tl)
        {
            return tl.ClientSize;
        }

        if (VisualRoot is Control c)
        {
            return c.Bounds.Size;
        }

        return default;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (e.Root is Control wb)
        {
            // OverlayLayer is a Canvas, so we won't get a signal to resize if the window
            // bounds change. Subscribe to force update
            _rootBoundsWatcher = wb.GetObservable(BoundsProperty)
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

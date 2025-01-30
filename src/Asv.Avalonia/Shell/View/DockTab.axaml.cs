using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

namespace Asv.Avalonia;

public class DockTab : TabItem
{
    //private Point _dragStartPosition;
    
    //private bool _isDragging;
    public DockTab()
    {
        Background = Brushes.LightGray; // Стиль вкладки
        Margin = new Thickness(5); // Отступы
    }

    // protected override void OnPointerPressed(PointerPressedEventArgs e)
    // {
    //     base.OnPointerPressed(e);
    //
    //     if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
    //     {
    //         _dragStartPosition = e.GetPosition(this);
    //         var parent = Parent as DockControl;
    //         parent?.StartDragging(this, e.GetPosition(parent));
    //         e.Pointer.Capture(this);
    //     }
    // }
    //
    // protected override void OnPointerMoved(PointerEventArgs e)
    // {
    //     base.OnPointerMoved(e);
    //     if (_isDragging)
    //     {
    //         var parent = Parent as DockControl;
    //         if (parent != null)
    //         {
    //             var currentPosition = e.GetPosition(parent);
    //             parent.OnTabDragged(this, currentPosition);
    //         }
    //     }
    // }
    //
    // protected override void OnPointerReleased(PointerReleasedEventArgs e)
    // {
    //     base.OnPointerReleased(e);
    //     e.Pointer.Capture(null);
    // }
}
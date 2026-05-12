using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Asv.Avalonia;

public class WindowResizeGrip : TemplatedControl
{
    public static readonly StyledProperty<double> ResizeBorderThicknessProperty =
        AvaloniaProperty.Register<WindowResizeGrip, double>(nameof(ResizeBorderThickness), 6);

    public static readonly StyledProperty<WindowEdge?> EdgeProperty = AvaloniaProperty.Register<
        WindowResizeGrip,
        WindowEdge?
    >(nameof(Edge));

    static WindowResizeGrip()
    {
        EdgeProperty.Changed.AddClassHandler<WindowResizeGrip>((x, _) => x.UpdateCursor());
        ResizeBorderThicknessProperty.Changed.AddClassHandler<WindowResizeGrip>((x, _) =>
            x.UpdateCursor()
        );
    }

    public WindowResizeGrip()
    {
        UpdateCursor();
    }

    public double ResizeBorderThickness
    {
        get => GetValue(ResizeBorderThicknessProperty);
        set => SetValue(ResizeBorderThicknessProperty, value);
    }

    public WindowEdge? Edge
    {
        get => GetValue(EdgeProperty);
        set => SetValue(EdgeProperty, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var parent = this as Visual;
        while (parent is not null and not Window)
        {
            parent = parent.GetVisualParent();
        }

        if (parent is not Window window)
        {
            return;
        }

        var edge = Edge ?? GetEdge(e.GetPosition(this));
        if (edge is null)
        {
            return;
        }

        if (window.WindowState is WindowState.Maximized or WindowState.FullScreen)
        {
            return;
        }

        window.BeginResizeDrag(edge.Value, e);
        e.Handled = true;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        var edge = Edge ?? GetEdge(e.GetPosition(this));
        Cursor = edge is null ? null : new Cursor(GetCursorType(edge.Value));
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        UpdateCursor();
    }

    private void UpdateCursor()
    {
        Cursor = Edge is { } edge ? new Cursor(GetCursorType(edge)) : null;
    }

    private WindowEdge? GetEdge(Point point)
    {
        var bounds = Bounds;
        var thickness = ResizeBorderThickness;

        if (point.Y <= thickness)
        {
            return WindowEdge.North;
        }

        if (point.Y >= bounds.Height - thickness)
        {
            return WindowEdge.South;
        }

        if (point.X <= thickness)
        {
            return WindowEdge.West;
        }

        if (point.X >= bounds.Width - thickness)
        {
            return WindowEdge.East;
        }

        return null;
    }

    private static StandardCursorType GetCursorType(WindowEdge edge)
    {
        return edge switch
        {
            WindowEdge.North => StandardCursorType.SizeNorthSouth,
            WindowEdge.South => StandardCursorType.SizeNorthSouth,
            WindowEdge.West => StandardCursorType.SizeWestEast,
            WindowEdge.East => StandardCursorType.SizeWestEast,
            WindowEdge.NorthWest => StandardCursorType.SizeAll,
            WindowEdge.SouthEast => StandardCursorType.SizeAll,
            WindowEdge.NorthEast => StandardCursorType.SizeAll,
            WindowEdge.SouthWest => StandardCursorType.SizeAll,
            _ => StandardCursorType.Arrow,
        };
    }
}

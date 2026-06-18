using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Asv.Avalonia;

public partial class DashboardView : UserControl
{
    private const double DragStartThreshold = 4;
    private const string DragOverClass = "drag-over";

    private Point _dragStartPoint;
    private ITileViewModel? _pressedTile;
    private bool _isDragging;

    public DashboardView()
    {
        InitializeComponent();
        AddHandler(
            InputElement.PointerPressedEvent,
            OnDashboardPointerPressed,
            RoutingStrategies.Tunnel,
            true
        );
        AddHandler(
            InputElement.PointerMovedEvent,
            OnDashboardPointerMoved,
            RoutingStrategies.Tunnel,
            true
        );
        AddHandler(
            InputElement.PointerReleasedEvent,
            OnDashboardPointerReleased,
            RoutingStrategies.Tunnel,
            true
        );
        AddHandler(
            InputElement.PointerCaptureLostEvent,
            OnDashboardPointerCaptureLost,
            RoutingStrategies.Tunnel,
            true
        );
    }

    private void OnDashboardPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            ResetDrag();
            return;
        }

        if (!TryFindTile(e.Source, out var tile))
        {
            ResetDrag();
            return;
        }

        _pressedTile = tile;
        _dragStartPoint = e.GetPosition(this);
        _isDragging = false;
    }

    private void OnDashboardPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_pressedTile is null)
        {
            return;
        }

        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            ResetDrag(e.Pointer);
            return;
        }

        var position = e.GetPosition(this);
        if (!_isDragging && !IsDragStarted(position))
        {
            return;
        }

        _isDragging = true;
        e.Pointer.Capture(this);
        UpdateDropTargetHighlight(position);
        e.Handled = true;
    }

    private void OnDashboardPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_pressedTile is null)
        {
            ResetDrag(e.Pointer);
            return;
        }

        if (_isDragging && TryGetDensityFromPoint(e.GetPosition(this), out var density))
        {
            _pressedTile.Density = density;
            e.Handled = true;
        }

        ResetDrag(e.Pointer);
    }

    private void OnDashboardPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        ResetDrag();
    }

    private bool IsDragStarted(Point position)
    {
        return Math.Abs(position.X - _dragStartPoint.X) >= DragStartThreshold
            || Math.Abs(position.Y - _dragStartPoint.Y) >= DragStartThreshold;
    }

    private void UpdateDropTargetHighlight(Point point)
    {
        foreach (var (target, _) in GetDropTargets())
        {
            if (ContainsPoint(target, point))
            {
                if (!target.Classes.Contains(DragOverClass))
                {
                    target.Classes.Add(DragOverClass);
                }
            }
            else
            {
                target.Classes.Remove(DragOverClass);
            }
        }
    }

    private bool TryGetDensityFromPoint(Point point, out TileDensity density)
    {
        foreach (var (target, targetDensity) in GetDropTargets())
        {
            if (ContainsPoint(target, point))
            {
                density = targetDensity;
                return true;
            }
        }

        density = default;
        return false;
    }

    private bool ContainsPoint(Control target, Point point)
    {
        var targetPosition = target.TranslatePoint(default, this);
        return targetPosition is { } position
            && new Rect(position, target.Bounds.Size).Contains(point);
    }

    private IEnumerable<(Control Target, TileDensity Density)> GetDropTargets()
    {
        yield return (RegularDropTarget, TileDensity.Regular);
        yield return (CompactDropTarget, TileDensity.Compact);
        yield return (InlineDropTarget, TileDensity.Inline);
    }

    private static bool TryFindTile(object? source, out ITileViewModel tile)
    {
        var control = source as Control;
        while (control is not null)
        {
            if (control.DataContext is ITileViewModel found)
            {
                tile = found;
                return true;
            }

            control = control.FindAncestorOfType<Control>();
        }

        tile = null!;
        return false;
    }

    private void ResetDrag(IPointer? pointer = null)
    {
        _pressedTile = null;
        _isDragging = false;
        pointer?.Capture(null);
        ClearDropTargetHighlight();
    }

    private void ClearDropTargetHighlight()
    {
        foreach (var (target, _) in GetDropTargets())
        {
            target.Classes.Remove(DragOverClass);
        }
    }
}

using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace Asv.Avalonia;

public partial class WorkspacePanel : Panel
{
    private readonly StackPanel _leftPanel;
    private readonly StackPanel _rightPanel;
    private readonly ScrollViewer _leftScrollViewer;
    private readonly ScrollViewer _rightScrollViewer;
    private readonly DockPanel _centerPanel;
    private readonly AvaloniaList<Control> _bottomPanel;
    private readonly ColumnDefinition _leftColumn;
    private readonly ColumnDefinition _centerColumn;
    private readonly ColumnDefinition _rightColumn;
    private readonly RowDefinition _centerRow;
    private readonly RowDefinition _bottomRow;
    private readonly GridSplitter _horizontalSplitter;
    private readonly GridSplitter _verticalSplitter1;
    private readonly GridSplitter _verticalSplitter2;
    private readonly RowDefinition _horizontalSplitterRaw;
    private readonly ColumnDefinition _verticalSplitterRaw1;
    private readonly ColumnDefinition _verticalSplitterRaw2;
    private IDisposable? _workspaceBindings;
    private readonly HashSet<Control> _trackedChildren = [];
    private bool _isLeftPaneShown;
    private bool _isRightPaneShown;

    static WorkspacePanel()
    {
        AffectsParentArrange<WorkspacePanel>(DockProperty);
    }

    public WorkspacePanel()
    {
        var mainGrid = new Grid { Name = "MainGrid", ShowGridLines = false };
        mainGrid.RowDefinitions = [new RowDefinition()];

        mainGrid.ColumnDefinitions =
        [
            _leftColumn = new ColumnDefinition
            {
                [!ColumnDefinition.WidthProperty] = this[!LeftWidthProperty],
            },
            _verticalSplitterRaw1 = new ColumnDefinition(
                new GridLength(WidgetSpacing, GridUnitType.Pixel)
            )
            {
                MaxWidth = WidgetSpacing,
            },
            _centerColumn = new ColumnDefinition
            {
                [!ColumnDefinition.WidthProperty] = this[!CentralWidthProperty],
                [!ColumnDefinition.MinWidthProperty] = this[!MinCentralWidthProperty],
            },
            _verticalSplitterRaw2 = new ColumnDefinition(
                new GridLength(WidgetSpacing, GridUnitType.Pixel)
            )
            {
                MaxWidth = WidgetSpacing,
            },
            _rightColumn = new ColumnDefinition
            {
                [!ColumnDefinition.WidthProperty] = this[!RightWidthProperty],
            },
        ];

        // The center/bottom rows live in a nested grid so that the side ScrollViewers sit in a
        // single Star row. If they spanned [Star, Pixel, Auto] rows instead (RowSpan=3), Grid would
        // measure them with unbounded height (cells touching an Auto row are measured before star
        // rows are resolved) — the ScrollViewers would size to content, never showing a scrollbar.
        var centerGrid = new Grid { Name = "CenterGrid", ShowGridLines = false };
        centerGrid.RowDefinitions =
        [
            _centerRow = new RowDefinition
            {
                [!RowDefinition.HeightProperty] = this[!CentralHeightProperty],
                [!RowDefinition.MinHeightProperty] = this[!MinCentralHeightProperty],
            },
            _horizontalSplitterRaw = new RowDefinition(
                new GridLength(WidgetSpacing, GridUnitType.Pixel)
            )
            {
                MaxHeight = WidgetSpacing,
            },
            _bottomRow = new RowDefinition
            {
                [!RowDefinition.HeightProperty] = this[!BottomHeightProperty],
                [!RowDefinition.MinHeightProperty] = this[!MinBottomHeightProperty],
            },
        ];
        Grid.SetColumn(centerGrid, 2);

        // Left ScrollViewer with the StackPanel
        _leftPanel = new StackPanel
        {
            Name = "PART_LeftPanel",
            [!StackPanel.SpacingProperty] = this[!WidgetSpacingProperty],
        };
        _leftScrollViewer = new ScrollViewer { Background = null, Content = _leftPanel };
        Grid.SetColumn(_leftScrollViewer, 0);

        // Right ScrollViewer with the StackPanel
        _rightPanel = new StackPanel
        {
            Name = "PART_RightPanel",
            [!StackPanel.SpacingProperty] = this[!WidgetSpacingProperty],
        };
        _rightScrollViewer = new ScrollViewer { Background = null, Content = _rightPanel };
        Grid.SetColumn(_rightScrollViewer, 4);

        // Bottom TabControl
        var bottomTab = new TabControl
        {
            Name = "PART_BottomTab",
            TabStripPlacement = Dock.Bottom,
            Background = null,
            Padding = new Thickness(0),
        };
        Grid.SetRow(bottomTab, 2);
        Grid.SetColumn(bottomTab, 0);
        bottomTab.ItemsSource = _bottomPanel = [];

        _centerPanel = new DockPanel
        {
            Name = "PART_CenterPanel",
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = null,
            LastChildFill = true,
        };

        Grid.SetRow(_centerPanel, 0);
        Grid.SetColumn(_centerPanel, 0);

        // Vertical GridSplitter between columns 0 and 2
        _verticalSplitter1 = new GridSplitter
        {
            Width = WidgetSpacing,
            ResizeDirection = GridResizeDirection.Columns,
            IsHitTestVisible = true,
            Cursor = new Cursor(StandardCursorType.SizeWestEast),
            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
        };
        _verticalSplitter1.DragCompleted += HorizontalSplitterOnDragCompleted;

        Grid.SetColumn(_verticalSplitter1, 1);

        // Vertical GridSplitter between columns 2 and 4
        _verticalSplitter2 = new GridSplitter
        {
            Width = WidgetSpacing,
            ResizeDirection = GridResizeDirection.Columns,
            IsHitTestVisible = true,
            Cursor = new Cursor(StandardCursorType.SizeWestEast),
            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
        };
        _verticalSplitter2.DragCompleted += HorizontalSplitterOnDragCompleted;

        Grid.SetColumn(_verticalSplitter2, 3);

        // Horizontal GridSplitter in row 2
        _horizontalSplitter = new GridSplitter
        {
            Height = WidgetSpacing,
            ResizeDirection = GridResizeDirection.Rows,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            IsHitTestVisible = true,
            Cursor = new Cursor(StandardCursorType.SizeNorthSouth),
            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
        };

        _horizontalSplitter.DragCompleted += HorizontalSplitterOnDragCompleted;
        Grid.SetRow(_horizontalSplitter, 1);
        Grid.SetColumn(_horizontalSplitter, 0);

        // Center column rows live in the nested grid
        centerGrid.Children.Add(_centerPanel);
        centerGrid.Children.Add(_horizontalSplitter);
        centerGrid.Children.Add(bottomTab);

        // Add all elements to the Grid
        mainGrid.Children.Add(_leftScrollViewer);
        mainGrid.Children.Add(_rightScrollViewer);
        mainGrid.Children.Add(centerGrid);
        mainGrid.Children.Add(_verticalSplitter1);
        mainGrid.Children.Add(_verticalSplitter2);

        // Add the Grid as the only child element of the panel
        LogicalChildren.Add(mainGrid);
        VisualChildren.Add(mainGrid);
        RefreshChildren();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == WidgetSpacingProperty)
        {
            UpdateWidgetSpacing();
        }
        else if (change.Property == TemplatedParentProperty)
        {
            BindWorkspaceProperties();
        }
        else if (change.Property == IsLeftPaneOpenProperty || change.Property == IsRightPaneOpenProperty)
        {
            RefreshChildren();
            InvalidateMeasure();
            RaiseWorkspaceChanged();
        }
        else if (change.Property == MinLeftWidthProperty || change.Property == MinRightWidthProperty)
        {
            UpdateSidePaneLayout();
            InvalidateMeasure();
        }
    }

    private void OnChildPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == DockProperty || change.Property == IsVisibleProperty)
        {
            // Style bindings may reset during logical detach; move children only in a layout pass.
            InvalidateMeasure();
        }
    }

    private void BindWorkspaceProperties()
    {
        _workspaceBindings?.Dispose();
        _workspaceBindings = null;
        if (TemplatedParent is not Workspace workspace)
        {
            return;
        }

        _workspaceBindings = R3.Disposable.Combine(
            Bind(IsLeftPaneOpenProperty, new Binding(nameof(Workspace.IsLeftPaneOpen))
            {
                Source = workspace,
                Mode = BindingMode.TwoWay,
            }),
            Bind(IsRightPaneOpenProperty, new Binding(nameof(Workspace.IsRightPaneOpen))
            {
                Source = workspace,
                Mode = BindingMode.TwoWay,
            })
        );
    }

    private void UpdateSidePaneLayout()
    {
        var leftShown = IsLeftPaneOpen && HasLeftPaneContent;
        var rightShown = IsRightPaneOpen && HasRightPaneContent;
        var leftChanged = _isLeftPaneShown != leftShown;
        var rightChanged = _isRightPaneShown != rightShown;

        if (leftChanged || rightChanged)
        {
            FreezeVisibleSideWidths();
        }

        _leftColumn.MinWidth = leftShown ? MinLeftWidth : 0;
        _leftColumn.MaxWidth = leftShown ? double.PositiveInfinity : 0;
        _rightColumn.MinWidth = rightShown ? MinRightWidth : 0;
        _rightColumn.MaxWidth = rightShown ? double.PositiveInfinity : 0;

        // Hidden Star columns can still participate in allocation and layout rounding.
        // Keep the public widths intact while giving hidden columns an explicit zero width.
        if (!leftShown || leftChanged)
        {
            _leftColumn.SetCurrentValue(
                ColumnDefinition.WidthProperty,
                leftShown ? LeftWidth : new GridLength(0)
            );
        }

        if (!rightShown || rightChanged)
        {
            _rightColumn.SetCurrentValue(
                ColumnDefinition.WidthProperty,
                rightShown ? RightWidth : new GridLength(0)
            );
        }

        _leftScrollViewer.IsVisible = leftShown;
        _rightScrollViewer.IsVisible = rightShown;
        _verticalSplitter1.IsVisible = leftShown;
        _verticalSplitter2.IsVisible = rightShown;
        _verticalSplitterRaw1.Width = leftShown
            ? new GridLength(WidgetSpacing, GridUnitType.Pixel)
            : GridLength.Auto;
        _verticalSplitterRaw2.Width = rightShown
            ? new GridLength(WidgetSpacing, GridUnitType.Pixel)
            : GridLength.Auto;
        _isLeftPaneShown = leftShown;
        _isRightPaneShown = rightShown;
    }

    private void FreezeVisibleSideWidths()
    {
        if (_isLeftPaneShown && _leftColumn.ActualWidth is > 0 and var left && double.IsFinite(left))
        {
            SetCurrentValue(LeftWidthProperty, new GridLength(left, GridUnitType.Pixel));
        }

        if (_isRightPaneShown && _rightColumn.ActualWidth is > 0 and var right && double.IsFinite(right))
        {
            SetCurrentValue(RightWidthProperty, new GridLength(right, GridUnitType.Pixel));
        }
    }

    private void UpdateWidgetSpacing()
    {
        var splitterLength = new GridLength(WidgetSpacing, GridUnitType.Pixel);

        _verticalSplitter1.Width = WidgetSpacing;
        _verticalSplitter2.Width = WidgetSpacing;
        _horizontalSplitter.Height = WidgetSpacing;

        _verticalSplitterRaw1.MaxWidth = WidgetSpacing;
        _verticalSplitterRaw2.MaxWidth = WidgetSpacing;
        _horizontalSplitterRaw.MaxHeight = WidgetSpacing;

        _verticalSplitterRaw1.Width = _verticalSplitter1.IsVisible
            ? splitterLength
            : GridLength.Auto;
        _verticalSplitterRaw2.Width = _verticalSplitter2.IsVisible
            ? splitterLength
            : GridLength.Auto;
        _horizontalSplitterRaw.Height = _horizontalSplitter.IsVisible
            ? splitterLength
            : GridLength.Auto;
    }

    internal double? LeftColumnPixelWidth =>
        GetPixelSize(_leftColumn.ActualWidth, LeftWidth);
    internal double? RightColumnPixelWidth =>
        GetPixelSize(_rightColumn.ActualWidth, RightWidth);
    internal double? BottomRowPixelHeight =>
        GetPixelSize(_bottomRow.ActualHeight, _bottomRow.Height);

    private static double? GetPixelSize(double actual, GridLength current)
    {
        if (actual > 0 && double.IsFinite(actual))
        {
            return actual;
        }

        return current is { GridUnitType: GridUnitType.Pixel, Value: > 0 } ? current.Value : null;
    }

    private void HorizontalSplitterOnDragCompleted(object? sender, VectorEventArgs e)
    {
        RaiseWorkspaceChanged();
    }

    private void RaiseWorkspaceChanged()
    {
        RaiseEvent(
            new WorkspaceEventArgs
            {
                LeftColumnActualWidth = _leftColumn.ActualWidth,
                CenterColumnActualWidth = _centerColumn.ActualWidth,
                RightColumnActualWidth = _rightColumn.ActualWidth,
                CenterRowActualHeight = _centerRow.ActualHeight,
                BottomRowActualHeight = _bottomRow.ActualHeight,
                Route = RoutingStrategies.Bubble,
                RoutedEvent = WorkspaceChangedEvent,
            }
        );
    }

    protected override void ChildrenChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            case NotifyCollectionChangedAction.Move:
            case NotifyCollectionChangedAction.Remove:
            case NotifyCollectionChangedAction.Replace:
                RefreshChildren();
                InvalidateMeasure();
                break;

            case NotifyCollectionChangedAction.Reset:
            default:
                throw new NotSupportedException();
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        RefreshChildren();
        return base.MeasureOverride(availableSize);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);
        RefreshChildren();
        return size;
    }

    private void RefreshChildren()
    {
        var current = Children.ToHashSet();
        foreach (var stale in _trackedChildren.Where(child => !current.Contains(child)).ToArray())
        {
            stale.PropertyChanged -= OnChildPropertyChanged;
            _trackedChildren.Remove(stale);
        }

        foreach (var child in current)
        {
            if (_trackedChildren.Add(child))
            {
                child.PropertyChanged += OnChildPropertyChanged;
            }
        }

        RemoveStale(_leftPanel.Children, current);
        RemoveStale(_rightPanel.Children, current);
        RemoveStale(_centerPanel.Children, current);
        RemoveStale(_bottomPanel, current);

        var leftIndex = 0;
        var rightIndex = 0;
        var bottomIndex = 0;
        var centerIndex = 0;

        foreach (var control in GetOrderedChildren())
        {
            var dock = GetDock(control);
            switch (dock)
            {
                case WorkspaceDock.Left:
                    MoveToPanel(
                        control,
                        _leftPanel.Children,
                        leftIndex++,
                        _rightPanel.Children,
                        _bottomPanel,
                        _centerPanel.Children
                    );
                    break;
                case WorkspaceDock.Right:
                    MoveToPanel(
                        control,
                        _rightPanel.Children,
                        rightIndex++,
                        _leftPanel.Children,
                        _bottomPanel,
                        _centerPanel.Children
                    );
                    break;
                case WorkspaceDock.Bottom:
                    MoveToPanel(
                        control,
                        _bottomPanel,
                        bottomIndex++,
                        _leftPanel.Children,
                        _rightPanel.Children,
                        _centerPanel.Children
                    );
                    break;
                case WorkspaceDock.Center:
                    MoveToPanel(
                        control,
                        _centerPanel.Children,
                        centerIndex++,
                        _leftPanel.Children,
                        _rightPanel.Children,
                        _bottomPanel
                    );
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        HasLeftPaneContent = _leftPanel.Children.Any(x => x.IsVisible);
        HasRightPaneContent = _rightPanel.Children.Any(x => x.IsVisible);
        UpdateSidePaneLayout();

        if (!_bottomPanel.Any(x => x.IsVisible))
        {
            _bottomRow.MaxHeight = 0;
            _horizontalSplitter.IsVisible = false;
            _horizontalSplitterRaw.Height = GridLength.Auto;
        }
        else
        {
            _bottomRow.MaxHeight = double.PositiveInfinity;
            _horizontalSplitter.IsVisible = true;
            _horizontalSplitterRaw.Height = new GridLength(WidgetSpacing, GridUnitType.Pixel);
        }

        for (int i = 0; i < _centerPanel.Children.Count; i++)
        {
            if (i < _centerPanel.Children.Count - 1)
            {
                DockPanel.SetDock(_centerPanel.Children[i], Dock.Top);
            }
            else
            {
                _centerPanel.Children[i].ClearValue(DockPanel.DockProperty);
            }
        }
    }

    private static void RemoveStale(IList<Control> panel, HashSet<Control> current)
    {
        for (var i = panel.Count - 1; i >= 0; i--)
        {
            if (!current.Contains(panel[i]))
            {
                panel.RemoveAt(i);
            }
        }
    }

    private IEnumerable<Control> GetOrderedChildren()
    {
        return OrderChildren(Children);
    }

    internal static IEnumerable<Control> OrderChildren(IEnumerable<Control> children)
    {
        return children
            .Select(
                (control, index) =>
                    new
                    {
                        Control = control,
                        Order = GetOrder(control),
                        index,
                    }
            )
            .OrderBy(x => x.Order)
            .ThenBy(x => x.index)
            .Select(x => x.Control);
    }

    private static int GetOrder(Control control)
    {
        return control.DataContext is IWorkspaceWidget widget ? widget.Order : 0;
    }

    private static void MoveToPanel(
        Control control,
        IList<Control> target,
        int targetIndex,
        params IList<Control>[] otherPanels
    )
    {
        foreach (var panel in otherPanels)
        {
            panel.Remove(control);
        }

        var currentIndex = target.IndexOf(control);
        if (currentIndex == targetIndex)
        {
            return;
        }

        if (currentIndex >= 0)
        {
            target.RemoveAt(currentIndex);
        }

        target.Insert(Math.Min(targetIndex, target.Count), control);
    }
}

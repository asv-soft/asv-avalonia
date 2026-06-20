using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;
using R3;

namespace Asv.Avalonia;

public partial class WorkspacePanel : Panel
{
    private readonly Subject<Unit> _layoutChanged = new();
    private readonly StackPanel _leftPanel;
    private readonly StackPanel _rightPanel;
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
    private WorkspacePanelConfig _config = new();
    private IDisposable? _layout;
    private bool _internalLayoutChange;

    static WorkspacePanel()
    {
        AffectsParentArrange<WorkspacePanel>(DockProperty);
    }

    public WorkspacePanel()
    {
        var mainGrid = new Grid { Name = "MainGrid", ShowGridLines = false };

        mainGrid.ColumnDefinitions =
        [
            _leftColumn = new ColumnDefinition
            {
                [!ColumnDefinition.WidthProperty] = this[!LeftWidthProperty],
                [!ColumnDefinition.MinWidthProperty] = this[!MinLeftWidthProperty],
            },
            _verticalSplitterRaw1 = new ColumnDefinition(
                new GridLength(SplitterSize, GridUnitType.Pixel)
            )
            {
                MaxWidth = SplitterSize,
            },
            _centerColumn = new ColumnDefinition
            {
                [!ColumnDefinition.WidthProperty] = this[!CentralWidthProperty],
                [!ColumnDefinition.MinWidthProperty] = this[!MinCentralWidthProperty],
            },
            _verticalSplitterRaw2 = new ColumnDefinition(
                new GridLength(SplitterSize, GridUnitType.Pixel)
            )
            {
                MaxWidth = SplitterSize,
            },
            _rightColumn = new ColumnDefinition
            {
                [!ColumnDefinition.WidthProperty] = this[!RightWidthProperty],
                [!ColumnDefinition.MinWidthProperty] = this[!MinRightWidthProperty],
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
                new GridLength(SplitterSize, GridUnitType.Pixel)
            )
            {
                MaxHeight = SplitterSize,
            },
            _bottomRow = new RowDefinition
            {
                [!RowDefinition.HeightProperty] = this[!BottomHeightProperty],
                [!RowDefinition.MinHeightProperty] = this[!MinBottomHeightProperty],
            },
        ];
        Grid.SetRow(centerGrid, 0);
        Grid.SetColumn(centerGrid, 2);

        // Left ScrollViewer with the StackPanel
        _leftPanel = new StackPanel { Name = "PART_LeftPanel", Spacing = 4 };
        var leftScrollViewer = new ScrollViewer { Background = null, Content = _leftPanel };
        Grid.SetRow(leftScrollViewer, 0);
        Grid.SetColumn(leftScrollViewer, 0);

        // Right ScrollViewer with the StackPanel
        _rightPanel = new StackPanel { Name = "PART_RightPanel", Spacing = 4 };
        var rightScrollViewer = new ScrollViewer { Background = null, Content = _rightPanel };
        Grid.SetRow(rightScrollViewer, 0);
        Grid.SetColumn(rightScrollViewer, 4);

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
            Width = SplitterSize,
            IsHitTestVisible = true,
            Cursor = Cursor.Parse("DragMove"),
            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
        };
        _verticalSplitter1.DragCompleted += HorizontalSplitterOnDragCompleted;

        Grid.SetRow(_verticalSplitter1, 0);
        Grid.SetColumn(_verticalSplitter1, 1);

        // Vertical GridSplitter between columns 2 and 4
        _verticalSplitter2 = new GridSplitter
        {
            Width = SplitterSize,
            IsHitTestVisible = true,
            Cursor = Cursor.Parse("DragMove"),
            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
        };
        _verticalSplitter2.DragCompleted += HorizontalSplitterOnDragCompleted;

        Grid.SetRow(_verticalSplitter2, 0);
        Grid.SetColumn(_verticalSplitter2, 3);

        // Horizontal GridSplitter in row 2
        _horizontalSplitter = new GridSplitter
        {
            Height = SplitterSize,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            IsHitTestVisible = true,
            Cursor = Cursor.Parse("DragMove"),
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
        mainGrid.Children.Add(leftScrollViewer);
        mainGrid.Children.Add(rightScrollViewer);
        mainGrid.Children.Add(centerGrid);
        mainGrid.Children.Add(_verticalSplitter1);
        mainGrid.Children.Add(_verticalSplitter2);

        // Add the Grid as the only child element of the panel
        LogicalChildren.Add(mainGrid);
        VisualChildren.Add(mainGrid);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        RegisterLayout();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _layout?.Dispose();
        _layout = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == LayoutIdProperty && _layout is not null)
        {
            RegisterLayout();
        }
    }

    private void HorizontalSplitterOnDragCompleted(object? sender, VectorEventArgs e)
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
        NotifyLayoutChanged(sender);
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
                break;

            case NotifyCollectionChangedAction.Reset:
            default:
                throw new NotSupportedException();
        }
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

        if (!_leftPanel.Children.Any(x => x.IsVisible))
        {
            _leftColumn.MaxWidth = 0;
            _verticalSplitter1.IsVisible = false;
            _verticalSplitterRaw1.Width = GridLength.Auto;
        }
        else
        {
            _leftColumn.MaxWidth = double.PositiveInfinity;
            _verticalSplitter1.IsVisible = true;
            _verticalSplitterRaw1.Width = new GridLength(SplitterSize, GridUnitType.Pixel);
        }

        if (!_rightPanel.Children.Any(x => x.IsVisible))
        {
            _rightColumn.MaxWidth = 0;
            _verticalSplitter2.IsVisible = false;
            _verticalSplitterRaw2.Width = GridLength.Auto;
        }
        else
        {
            _rightColumn.MaxWidth = double.PositiveInfinity;
            _verticalSplitter2.IsVisible = true;
            _verticalSplitterRaw2.Width = new GridLength(SplitterSize, GridUnitType.Pixel);
        }

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
            _horizontalSplitterRaw.Height = new GridLength(SplitterSize, GridUnitType.Pixel);
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

    private void RegisterLayout()
    {
        _layout?.Dispose();
        _layout = null;

        if (Design.IsDesignMode || string.IsNullOrWhiteSpace(LayoutId))
        {
            return;
        }

        _layout = this.RegisterLayout(LayoutId, LoadLayout, SaveLayout, _layoutChanged);
    }

    private void LoadLayout(WorkspacePanelConfig config)
    {
        _internalLayoutChange = true;
        try
        {
            _config = config;

            if (TryGetValidPixelWidth(config.LeftColumnWidth, MinLeftWidth, out var leftWidth))
            {
                LeftWidth = new GridLength(leftWidth, GridUnitType.Pixel);
            }

            if (TryGetValidPixelWidth(config.RightColumnWidth, MinRightWidth, out var rightWidth))
            {
                RightWidth = new GridLength(rightWidth, GridUnitType.Pixel);
            }
        }
        finally
        {
            _internalLayoutChange = false;
        }
    }

    private WorkspacePanelConfig? SaveLayout()
    {
        if (_internalLayoutChange)
        {
            return null;
        }

        var config = new WorkspacePanelConfig
        {
            LeftColumnWidth = _config.LeftColumnWidth,
            RightColumnWidth = _config.RightColumnWidth,
        };

        if (GetColumnPixelWidth(_leftColumn, LeftWidth) is { } leftWidth)
        {
            config.LeftColumnWidth = leftWidth;
        }

        if (GetColumnPixelWidth(_rightColumn, RightWidth) is { } rightWidth)
        {
            config.RightColumnWidth = rightWidth;
        }

        if (config.LeftColumnWidth is null && config.RightColumnWidth is null)
        {
            return null;
        }

        _config = config;
        return config;
    }

    private void NotifyLayoutChanged(object? sender)
    {
        if (
            _internalLayoutChange
            || (
                !ReferenceEquals(sender, _verticalSplitter1)
                && !ReferenceEquals(sender, _verticalSplitter2)
            )
        )
        {
            return;
        }

        _layoutChanged.OnNext(Unit.Default);
    }

    private static double? GetColumnPixelWidth(ColumnDefinition column, GridLength fallback)
    {
        if (IsValidPixelWidth(column.ActualWidth))
        {
            return column.ActualWidth;
        }

        return fallback.GridUnitType == GridUnitType.Pixel && IsValidPixelWidth(fallback.Value)
            ? fallback.Value
            : null;
    }

    private static bool TryGetValidPixelWidth(double? value, double minWidth, out double width)
    {
        width = 0;
        if (value is not { } pixelWidth || !IsValidPixelWidth(pixelWidth))
        {
            return false;
        }

        if (double.IsFinite(minWidth) && minWidth > 0)
        {
            pixelWidth = Math.Max(pixelWidth, minWidth);
        }

        width = pixelWidth;
        return true;
    }

    private static bool IsValidPixelWidth(double value)
    {
        return value is > 0 && double.IsFinite(value);
    }
}

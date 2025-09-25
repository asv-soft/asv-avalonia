using System.Collections.Specialized;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;

namespace Asv.Avalonia;

public partial class WorkspacePanel : Panel
{
    private readonly StackPanel _leftPanel;
    private readonly StackPanel _rightPanel;
    private readonly DockPanel _centerPanel;
    private readonly AvaloniaList<Control> _bottomPanel;
    private readonly ColumnDefinition _leftColumn;
    private readonly ColumnDefinition _centerColumn;
    private readonly ColumnDefinition _rightColumn;
    private readonly RowDefinition _centerRow;
    private readonly RowDefinition _bottomRow;

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
            new ColumnDefinition(new GridLength(5, GridUnitType.Pixel)) { MaxWidth = 5 },
            _centerColumn = new ColumnDefinition
            {
                [!ColumnDefinition.WidthProperty] = this[!CentralWidthProperty],
                [!ColumnDefinition.MinWidthProperty] = this[!MinCentralWidthProperty],
            },
            new ColumnDefinition(new GridLength(5, GridUnitType.Pixel)) { MaxWidth = 5 },
            _rightColumn = new ColumnDefinition
            {
                [!ColumnDefinition.WidthProperty] = this[!RightWidthProperty],
                [!ColumnDefinition.MinWidthProperty] = this[!MinRightWidthProperty],
            },
        ];

        mainGrid.RowDefinitions =
        [
            _centerRow = new RowDefinition
            {
                [!RowDefinition.HeightProperty] = this[!CentralHeightProperty],
                [!RowDefinition.MinHeightProperty] = this[!MinCentralHeightProperty],
            },
            new RowDefinition(new GridLength(5, GridUnitType.Pixel)) { MaxHeight = 5 },
            _bottomRow = new RowDefinition
            {
                [!RowDefinition.HeightProperty] = this[!BottomHeightProperty],
                [!RowDefinition.MinHeightProperty] = this[!MinBottomHeightProperty],
            },
        ];

        // Left ScrollViewer with the StackPanel
        _leftPanel = new StackPanel { Name = "PART_LeftPanel", Spacing = 4 };
        var leftScrollViewer = new ScrollViewer
        {
            //Margin = new Thickness(4, 4, 0, 4),
            Background = null,
            Content = _leftPanel,
        };
        Grid.SetRow(leftScrollViewer, 0);
        Grid.SetColumn(leftScrollViewer, 0);
        Grid.SetRowSpan(leftScrollViewer, 3);

        // Right ScrollViewer with the StackPanel
        _rightPanel = new StackPanel { Name = "PART_RightPanel", Spacing = 4 };
        var rightScrollViewer = new ScrollViewer
        {
            //Margin = new Thickness(0, 4, 4, 4),
            Background = null,
            Content = _rightPanel,
        };
        Grid.SetRow(rightScrollViewer, 0);
        Grid.SetColumn(rightScrollViewer, 4);
        Grid.SetRowSpan(rightScrollViewer, 3);

        // Bottom TabControl
        var bottomTab = new TabControl
        {
            Name = "PART_BottomTab",
            TabStripPlacement = Dock.Bottom,
            Background = null,
        };
        Grid.SetRow(bottomTab, 2);
        Grid.SetColumn(bottomTab, 2);
        bottomTab.ItemsSource = _bottomPanel = new AvaloniaList<Control>();

        _centerPanel = new DockPanel
        {
            Name = "PART_CenterPanel",
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = Brushes.Transparent,
            LastChildFill = true,
        };

        Grid.SetRow(_centerPanel, 0);
        Grid.SetColumn(_centerPanel, 2);

        // Vertical GridSplitter between columns 0 and 2
        var verticalSplitter1 = new GridSplitter
        {
            Width = 5,
            Background = Brushes.Transparent,
            IsHitTestVisible = true,
            Cursor = Cursor.Parse("SizeAll"),
            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
        };
        verticalSplitter1.DragCompleted += HorizontalSplitterOnDragCompleted;

        Grid.SetRow(verticalSplitter1, 0);
        Grid.SetRowSpan(verticalSplitter1, 3);
        Grid.SetColumn(verticalSplitter1, 1);

        // Vertical GridSplitter between columns 2 and 4
        var verticalSplitter2 = new GridSplitter
        {
            Width = 5,
            Background = Brushes.Transparent,
            IsHitTestVisible = true,
            Opacity = 1,
            Cursor = Cursor.Parse("SizeAll"),
            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
        };
        verticalSplitter2.DragCompleted += HorizontalSplitterOnDragCompleted;

        Grid.SetRow(verticalSplitter2, 0);
        Grid.SetRowSpan(verticalSplitter2, 3);
        Grid.SetColumn(verticalSplitter2, 3);

        // Horizontal GridSplitter in row 2
        var horizontalSplitter = new GridSplitter
        {
            Height = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = Brushes.Transparent,
            IsHitTestVisible = true,
            Cursor = Cursor.Parse("SizeAll"),
            ResizeBehavior = GridResizeBehavior.PreviousAndNext,
        };
        horizontalSplitter.DragCompleted += HorizontalSplitterOnDragCompleted;
        Grid.SetRow(horizontalSplitter, 2);
        Grid.SetColumn(horizontalSplitter, 1);
        Grid.SetColumnSpan(horizontalSplitter, 3);

        // Add all elements to the Grid
        mainGrid.Children.Add(leftScrollViewer);
        mainGrid.Children.Add(rightScrollViewer);
        mainGrid.Children.Add(bottomTab);
        mainGrid.Children.Add(_centerPanel);
        mainGrid.Children.Add(verticalSplitter1);
        mainGrid.Children.Add(verticalSplitter2);
        mainGrid.Children.Add(horizontalSplitter);

        // Add the Grid as the only child element of the panel
        LogicalChildren.Add(mainGrid);
        VisualChildren.Add(mainGrid);
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
        RefreshChildren();
        return base.ArrangeOverride(finalSize);
    }

    private void RefreshChildren()
    {
        foreach (var control in Children)
        {
            var dock = GetDock(control);
            switch (dock)
            {
                case WorkspaceDock.Left:
                    if (_leftPanel.Children.Contains(control))
                    {
                        continue;
                    }

                    _rightPanel.Children.Remove(control);
                    _bottomPanel.Remove(control);
                    _centerPanel.Children.Remove(control);
                    _leftPanel.Children.Add(control);
                    break;
                case WorkspaceDock.Right:
                    if (_rightPanel.Children.Contains(control))
                    {
                        continue;
                    }

                    _leftPanel.Children.Remove(control);
                    _bottomPanel.Remove(control);
                    _centerPanel.Children.Remove(control);
                    _rightPanel.Children.Add(control);
                    break;
                case WorkspaceDock.Bottom:
                    if (_bottomPanel.Contains(control))
                    {
                        continue;
                    }

                    _leftPanel.Children.Remove(control);
                    _rightPanel.Children.Remove(control);
                    _centerPanel.Children.Remove(control);
                    _bottomPanel.Add(control);
                    break;
                case WorkspaceDock.Center:
                    if (_centerPanel.Children.Contains(control))
                    {
                        continue;
                    }
                    _leftPanel.Children.Remove(control);
                    _rightPanel.Children.Remove(control);
                    _bottomPanel.Remove(control);
                    _centerPanel.Children.Add(control);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
}

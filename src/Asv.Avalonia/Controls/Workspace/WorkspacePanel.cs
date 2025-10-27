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
    private readonly GridSplitter _horizontalSplitter;
    private readonly GridSplitter _verticalSplitter1;
    private readonly GridSplitter _verticalSplitter2;
    private readonly RowDefinition _horizontalSplitterRaw;
    private readonly ColumnDefinition _verticalSplitterRaw1;
    private readonly ColumnDefinition _verticalSplitterRaw2;

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

        mainGrid.RowDefinitions =
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

        // Left ScrollViewer with the StackPanel
        _leftPanel = new StackPanel { Name = "PART_LeftPanel", Spacing = 4 };
        var leftScrollViewer = new ScrollViewer { Background = null, Content = _leftPanel };
        Grid.SetRow(leftScrollViewer, 0);
        Grid.SetColumn(leftScrollViewer, 0);
        Grid.SetRowSpan(leftScrollViewer, 3);

        // Right ScrollViewer with the StackPanel
        _rightPanel = new StackPanel { Name = "PART_RightPanel", Spacing = 4 };
        var rightScrollViewer = new ScrollViewer { Background = null, Content = _rightPanel };
        Grid.SetRow(rightScrollViewer, 0);
        Grid.SetColumn(rightScrollViewer, 4);
        Grid.SetRowSpan(rightScrollViewer, 3);

        // Bottom TabControl
        var bottomTab = new TabControl
        {
            Name = "PART_BottomTab",
            TabStripPlacement = Dock.Bottom,
            Background = null,
            Padding = new Thickness(0),
        };
        Grid.SetRow(bottomTab, 2);
        Grid.SetColumn(bottomTab, 2);
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
        Grid.SetColumn(_centerPanel, 2);

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
        Grid.SetRowSpan(_verticalSplitter1, 3);
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
        Grid.SetRowSpan(_verticalSplitter2, 3);
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
        Grid.SetColumn(_horizontalSplitter, 2);

        // Add all elements to the Grid
        mainGrid.Children.Add(leftScrollViewer);
        mainGrid.Children.Add(rightScrollViewer);
        mainGrid.Children.Add(bottomTab);
        mainGrid.Children.Add(_centerPanel);
        mainGrid.Children.Add(_verticalSplitter1);
        mainGrid.Children.Add(_verticalSplitter2);
        mainGrid.Children.Add(_horizontalSplitter);

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

        if (!_leftPanel.Children.Any(x => x.IsVisible))
        {
            _leftColumn.Width = GridLength.Auto;
            _leftColumn.MinWidth = 0;
            _verticalSplitter1.IsVisible = false;
            _verticalSplitterRaw1.Width = GridLength.Auto;
        }
        else
        {
            _leftColumn.Width = LeftWidth;
            _leftColumn.MinWidth = MinLeftWidth;
            _verticalSplitter1.IsVisible = true;
            _verticalSplitterRaw1.Width = new GridLength(SplitterSize, GridUnitType.Pixel);
        }

        if (!_rightPanel.Children.Any(x => x.IsVisible))
        {
            _rightColumn.Width = GridLength.Auto;
            _rightColumn.MinWidth = 0;
            _verticalSplitter2.IsVisible = false;
            _verticalSplitterRaw2.Width = GridLength.Auto;
        }
        else
        {
            _rightColumn.Width = LeftWidth;
            _rightColumn.MinWidth = MinLeftWidth;
            _verticalSplitter2.IsVisible = true;
            _verticalSplitterRaw2.Width = new GridLength(SplitterSize, GridUnitType.Pixel);
        }

        if (!_bottomPanel.Any(x => x.IsVisible))
        {
            _bottomRow.Height = GridLength.Auto;
            _horizontalSplitter.IsVisible = false;
            _horizontalSplitterRaw.Height = GridLength.Auto;
        }
        else
        {
            _bottomRow.Height = BottomHeight;
            _bottomRow.MinHeight = MinBottomHeight;
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
}

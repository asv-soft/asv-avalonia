using System.Collections.Specialized;
using Asv.Common;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Asv.Avalonia;

public class ShellItem
{
    public TabItem TabControl { get; init; } = new();
    public int Column { get; set; }
}

public class DockControl : SelectingItemsControl
{
    private readonly List<Border> _targetBorders = [];
    private readonly List<ShellItem> _shellItems = [];
    private TabItem? _selectedTab;
    private Border? _leftSelector;
    private Border? _rightSelector;
    private Grid? _dropTargetGrid;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ItemCountProperty)
        {
            CreateTabs();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _leftSelector = e.NameScope.Find<Border>("PART_LeftSelector");
        _rightSelector = e.NameScope.Find<Border>("PART_RightSelector");
        _dropTargetGrid = e.NameScope.Find<Grid>("PART_DockSelectivePart");
        if (_dropTargetGrid is null)
        {
            throw new ApplicationException();
        }

        if (_leftSelector != null)
        {
            _targetBorders.Add(_leftSelector);
        }

        if (_rightSelector != null)
        {
            _targetBorders.Add(_rightSelector);
        }

        CreateTabs();
    }

    #region TabDockEvents

    private void PressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var source = e.Source as Visual;
        var tab = source is TabItem item ? item : source?.FindAncestorOfType<TabItem>();
        if (tab != null)
        {
            _selectedTab = tab;
        }

        e.Pointer.Capture(this);
    }

    private void PointerMovedHandler(object? sender, PointerEventArgs e)
    {
        if (_selectedTab == null)
        {
            return;
        }

        var pointerPosition = e.GetPosition(this);

        foreach (var border in _targetBorders)
        {
            border.Background = IsCursorWithinTargetBorder(pointerPosition, border)
                ? Brushes.LightBlue
                : Brushes.Transparent;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (_dropTargetGrid == null)
        {
            return;
        }

        foreach (var item in _dropTargetGrid.Children.OfType<AdaptiveTabStripTabControl>())
        {
            item.BorderBrush = Brushes.Transparent;
        }

        base.OnPointerMoved(e);
        var isBorderSelected = false;
        if (_selectedTab == null)
        {
            return;
        }

        var pointerPosition = e.GetPosition(this);

        foreach (var border in _targetBorders)
        {
            border.Background = IsCursorWithinTargetBorder(pointerPosition, border)
                ? Brushes.LightBlue
                : Brushes.Transparent;
            if (IsCursorWithinTargetBorder(pointerPosition, border))
            {
                isBorderSelected = true;
            }
        }

        if (_dropTargetGrid == null)
        {
            return;
        }

        if (isBorderSelected)
        {
            return;
        }

        foreach (var item in _dropTargetGrid.Children.OfType<AdaptiveTabStripTabControl>())
        {
            if (_selectedTab.FindAncestorOfType<AdaptiveTabStripTabControl>() == item)
            {
                continue;
            }

            item.BorderBrush = IsCursorWithinTabControl(pointerPosition, item)
                ? Brushes.LightBlue
                : Brushes.Transparent;
            item.BorderThickness = new Thickness(IsCursorWithinTabControl(pointerPosition, item) ? 2 : 0);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        foreach (var border in _targetBorders)
        {
            border.Background = Brushes.Transparent;
        }

        if (_selectedTab == null)
        {
            return;
        }

        if (this.GetVisualRoot() is not Window window)
        {
            return;
        }

        var cursorPosition = e.GetPosition(window);
        foreach (var targetBorder in _targetBorders)
        {
            if (IsCursorWithinTargetBorder(cursorPosition, targetBorder))
            {
                AddTabItemToTabControl(_selectedTab, targetBorder);
                break;
            }

            if (IsCursorWithinDockControl(cursorPosition))
            {
                continue;
            }

            var parent = _selectedTab.FindAncestorOfType<AdaptiveTabStripTabControl>();
            if (parent is null)
            {
                return;
            }

            parent.Items.Remove(_selectedTab);
            var win = new DockWindow()
            {
                Content = _selectedTab.Content,
                Title = (_selectedTab.Header as TabStripItem)?.Content?.ToString(),
            };
            win.Show();
        }

        foreach (var child in _dropTargetGrid!.Children)
        {
            if (child is not AdaptiveTabStripTabControl tabControl)
            {
                continue;
            }

            if (!IsCursorWithinTabControl(cursorPosition, tabControl))
            {
                continue;
            }

            if (_selectedTab.FindAncestorOfType<AdaptiveTabStripTabControl>() == tabControl)
            {
                continue;
            }

            _shellItems.Find(item => item.TabControl == _selectedTab)!.Column = Grid.GetColumn(tabControl);
            UpdateGrid();
            break;
        }

        _selectedTab = null;
    }

    // private void WinOnPointerReleased(object? sender, PointerReleasedEventArgs e)
    // {
    //     if (_isWindowSelected == false)
    //     {
    //         return;
    //     }
    //
    //     var position = e.GetCurrentPoint(this).Position;
    //     foreach (var border in _targetBorders.Where(border => IsCursorWithinTargetBorder(position, border)))
    //     {
    //         MoveWindowTabBack(sender);
    //     }
    // }
    //
    // private void CheckWindowPosition(PixelPointEventArgs e)
    // {
    //     if (_isWindowSelected == false)
    //     {
    //         return;
    //     }
    //
    //     foreach (var border in _targetBorders)
    //     {
    //         if (IsCursorWithinTargetBorder(new Point(e.Point.X, e.Point.Y), border))
    //         {
    //             border.Background = Brushes.LightBlue;
    //             _windowCloseRequest = true;
    //             _targetBorder = border;
    //         }
    //         else
    //         {
    //             border.Background = Brushes.Transparent;
    //         }
    //     }
    // }

    // private void MoveWindowTabBack(object? sender)
    // {
    //     // if (_isWindowSelected == false)
    //     // {
    //     //     return;
    //     // }
    //     var win = sender as DockWindow;
    //     if (!_windowCloseRequest)
    //     {
    //         return;
    //     }
    //
    //     {
    //         var tab = CreateTabItem(win!);
    //
    //         AddTabItemToTabControl(tab, _targetBorder);
    //         win!.CloseRequested = true;
    //     }
    //
    //     _windowCloseRequest = false;
    // }
    #endregion

    protected override void LogicalChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.LogicalChildrenCollectionChanged(sender, e);
        CreateTabs();
    }

    private void CreateTabs()
    {
        if(Items.Count == 0)
        {
            return;
        }
        
        if (_dropTargetGrid is null)
        {
            throw new ElementNotEnabledException();
        }

        foreach (var content in Items)
        {
            if (content is null)
            {
                continue;
            }

            var shellItem = new ShellItem()
            {
                TabControl = CreateTabItem(content),
            };
            if (_shellItems.Contains(shellItem))
            {
                continue;
            }

            _shellItems.Add(shellItem);
        }

        UpdateGrid();
    }

    private void UpdateGrid()
    {
        _dropTargetGrid!.Children.Clear();
        _dropTargetGrid.ColumnDefinitions.Clear();
        if (_shellItems.Min(_ => _.Column) != 0 && _shellItems.All(_ => _.Column == _shellItems[0].Column))
        {
            foreach (var item in _shellItems)
            {
                item.Column = 0;
            }
        }

        _shellItems.OrderBy(_ => _.Column);

        GenerateColumns(_dropTargetGrid, _shellItems.ToArray());

        var occupiedColumns = _shellItems.Select(item => item.Column).ToHashSet();

        bool ColumnHasGridSplitter(int columnIndex)
        {
            return _dropTargetGrid.Children
                .OfType<GridSplitter>()
                .Any(splitter => Grid.GetColumn(splitter) == columnIndex);
        }

        for (var i = 0; i < _dropTargetGrid.ColumnDefinitions.Count; i++)
        {
            if (!occupiedColumns.Contains(i) && !ColumnHasGridSplitter(i))
            {
                _dropTargetGrid.ColumnDefinitions[i].Width = new GridLength(0, GridUnitType.Pixel);
            }
        }

        foreach (var item in _shellItems)
        {
            var tabControl = FindTabControlInColumn(_dropTargetGrid, item.Column) ?? new AdaptiveTabStripTabControl();

            var oldParent = tabControl.Parent as Grid;
            oldParent?.Children.Remove(tabControl);

            var parent = item.TabControl.FindAncestorOfType<AdaptiveTabStripTabControl>();
            parent?.Items.Remove(item.TabControl);

            tabControl.Items.Add(item.TabControl);
            Grid.SetColumn(tabControl, item.Column);
            _dropTargetGrid.Children.Add(tabControl);
        }
    }

    private void GenerateColumns(Grid grid, ShellItem[] items)
    {
        grid.ColumnDefinitions.Clear();
        grid.Children.Clear();
        for (var i = 0; i < items.Length; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

            if (i >= items.Length - 1)
            {
                continue;
            }

            if (items.All(c => c.Column == items[i].Column))
            {
                break;
            }

            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Auto));

            var splitter = new GridSplitter()
            {
                Background = Brushes.White,
                Width = 1,
                ResizeDirection = GridResizeDirection.Columns,
            };

            Grid.SetColumn(splitter, grid.ColumnDefinitions.Count - 1);
            grid.Children.Add(splitter);
        }
    }

    private AdaptiveTabStripTabControl? FindTabControlInColumn(Grid myGrid, int columnIndex)
    {
        foreach (var child in myGrid.Children)
        {
            if (child is AdaptiveTabStripTabControl tabControl && Grid.GetColumn(tabControl) == columnIndex)
            {
                return tabControl;
            }
        }

        return null;
    }

    private TabItem CreateTabItem(object content)
    {
        var header = new TabStripItem()
        {
            Content = (content as IPage)?.Title.Value ?? "Tab",
        };
        header.PointerPressed += PressedHandler;
        header.PointerMoved += PointerMovedHandler;
        var tab = new TabItem()
        {
            Content = content,
            Header = header,
        };
        return tab;
    }

    #region HelperMethods

    private bool IsCursorWithinTargetBorder(Point cursorPosition, Border targetBorder)
    {
        return targetBorder.Bounds.Contains(cursorPosition);
    }

    private bool IsCursorWithinTabControl(Point cursorPosition, AdaptiveTabStripTabControl targetPanel)
    {
        return targetPanel.Bounds.Contains(cursorPosition);
    }

    private bool IsCursorWithinDockControl(Point cursorPosition)
    {
        var window = this.GetVisualRoot() as Window;
        return window!.Bounds.Contains(cursorPosition);
    }

    private void AddTabItemToTabControl(TabItem tabItem, Border selectorBorder)
    {
        var updateItem = _shellItems.Find(shellItem => shellItem.TabControl == tabItem);
        if (updateItem == null)
        {
            return;
        }

        var maxColumnIndex = MaxSplitAmount * 2;

        if (selectorBorder == _leftSelector)
        {
            updateItem.Column -= 2;

            if (_shellItems.Count(shellItem => shellItem.Column.Equals(updateItem.Column)) >= 2)
            {
                updateItem.Column -= 2;
            }

            if (updateItem.Column < 0)
            {
                foreach (var item in _shellItems)
                {
                    item.Column += 2;
                }

                updateItem.Column = 0;
            }
        }
        else if (selectorBorder == _rightSelector)
        {
            updateItem.Column += 2;

            if (_shellItems.Count(shellItem => shellItem.Column.Equals(updateItem.Column)) >= 2)
            {
                updateItem.Column += 2;
            }
        }

        if (_shellItems.MinItem(shellItem => shellItem.Column).Column != 0)
        {
            foreach (var item in _shellItems)
            {
                item.Column -= 2;
            }
        }

        if (_shellItems.MaxItem(shellItem => shellItem.Column).Column >= maxColumnIndex)
        {
            _shellItems.MaxItem(shellItem => shellItem.Column).Column -= 2;
        }

        UpdateGrid();
    }

    #endregion

    public static readonly StyledProperty<int?> MaxSplitAmountProperty =
        AvaloniaProperty.Register<DockControl, int?>(nameof(MaxSplitAmount), 4);

    public int? MaxSplitAmount
    {
        get => GetValue(MaxSplitAmountProperty);
        set => SetValue(MaxSplitAmountProperty, value);
    }

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentControl.ContentTemplateProperty.AddOwner<TabControl>();

    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }
}
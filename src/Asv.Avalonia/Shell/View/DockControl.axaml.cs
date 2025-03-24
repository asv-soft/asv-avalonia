using System.Collections.Specialized;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;
using R3;

namespace Asv.Avalonia;

public class ShellItem
{
    public TabItem TabControl { get; init; } = new();
    public int Column { get; set; }
}

public class DockControl : SelectingItemsControl
{
    private const int ColumnIncrement = 2;
    private readonly List<Border> _targetBorders = [];
    private List<ShellItem> _shellItems = [];
    private TabItem? _selectedTab;
    private Border _leftSelector;
    private Border _rightSelector;
    private Grid _dropTargetGrid;

    public DockControl()
    {
        UnSplitAllCommand = new ReactiveCommand(_ => UnsplitAll());
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        UnSplitAllCommand.Dispose();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ItemCountProperty)
        {
            CreateTabs();
        }

        if (change.Property == SelectedItemProperty)
        {
            _shellItems.First(_ => _.TabControl.Content == change.NewValue).TabControl.IsSelected =
                true;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _leftSelector =
            e.NameScope.Find<Border>("PART_LeftSelector")
            ?? throw new ApplicationException(
                "Part of control PART_LeftSelector not found in DockControl.axaml"
            );
        _rightSelector =
            e.NameScope.Find<Border>("PART_RightSelector")
            ?? throw new ApplicationException(
                "Part of control PART_RightSelector not found in DockControl.axaml"
            );
        _dropTargetGrid =
            e.NameScope.Find<Grid>("PART_DockSelectivePart")
            ?? throw new ApplicationException(
                "Part of control PART_DockSelectivePart not found in DockControl.axaml"
            );

        _targetBorders.Add(_leftSelector);
        _targetBorders.Add(_rightSelector);
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
        var tab = source as TabItem ?? source?.FindAncestorOfType<TabItem>();
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
            border.Background = TurnBorderIndicator(
                IsCursorWithinTargetBorder(pointerPosition, border)
            );
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        if (_selectedTab == null)
        {
            return;
        }

        foreach (var item in _dropTargetGrid.Children.OfType<AdaptiveTabStripTabControl>())
        {
            item.BorderBrush = Brushes.Transparent;
        }

        var isBorderSelected = false;

        var pointerPosition = e.GetPosition(this);

        foreach (var border in _targetBorders)
        {
            border.Background = TurnBorderIndicator(
                IsCursorWithinTargetBorder(pointerPosition, border)
            );
            if (IsCursorWithinTargetBorder(pointerPosition, border))
            {
                isBorderSelected = true;
            }
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

            item.BorderBrush = TurnBorderIndicator(IsCursorWithinTabControl(pointerPosition, item));

            item.BorderThickness = new Thickness(
                IsCursorWithinTabControl(pointerPosition, item) ? 4 : 0
            );
        }
    }

    private IBrush TurnBorderIndicator(bool turnOn)
    {
        return turnOn ? BorderHighLightColor : Brushes.Transparent;
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

            if (IsCursorOutOfDockControl(cursorPosition))
            {
                continue;
            }

            var parent = _selectedTab.FindAncestorOfType<AdaptiveTabStripTabControl>();
            if (parent is null)
            {
                return;
            }

            parent.Items.Remove(_selectedTab);
            var win = new Window()
            {
                Content = _selectedTab.Content,
                Title = (_selectedTab.Header as TabStripItem)?.Content?.ToString(),
            };
            win.Show();
            _selectedTab = null;
            return;
        }

        foreach (var child in _dropTargetGrid.Children)
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

            _shellItems.First(item => item.TabControl == _selectedTab).Column = Grid.GetColumn(
                tabControl
            );
            UpdateGrid();
            break;
        }

        _selectedTab = null;
    }

    #endregion

    protected override void LogicalChildrenCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e
    )
    {
        base.LogicalChildrenCollectionChanged(sender, e);
        if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            if (e.OldItems != null)
            {
                foreach (var removedItem in e.OldItems)
                {
                    var shellItem = _shellItems.FirstOrDefault(item =>
                        item.TabControl.Content == removedItem
                    );
                    if (shellItem != null)
                    {
                        _shellItems.Remove(shellItem);
                    }
                }
            }
        }

        UpdateGrid();
    }

    private void CreateTabs()
    {
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
        var existingItems = _shellItems.ToDictionary(
            key => key.TabControl.Content,
            value => value.Column
        );
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
        _shellItems.ForEach(_ => _.TabControl.Loaded -= OnTabLoaded);
        _shellItems.Clear();

        if (Items.Count == 0)
        {
            return;
        }

        if (_dropTargetGrid is null)
        {
            throw new ArgumentNullException(
                $"_dropTargetGrid in {nameof(DockControl)} is not found"
            );
        }

        foreach (var content in Items)
        {
            if (content is null)
            {
                continue;
            }

            var column = existingItems.GetValueOrDefault(content, 0);

            var shellItem = new ShellItem { TabControl = CreateTabItem(content), Column = column };

            _shellItems.Add(shellItem);
        }

        UpdateGrid();
    }

    private void UpdateGrid()
    {
        SortShellItems();

        _dropTargetGrid.Children.Clear();
        _dropTargetGrid.ColumnDefinitions.Clear();

        var occupiedColumns = _shellItems.Select(item => item.Column).ToHashSet();
        GenerateColumns(_dropTargetGrid, occupiedColumns);

        foreach (var item in _shellItems)
        {
            var tabControl =
                FindTabControlInColumn(_dropTargetGrid, item.Column)
                ?? new AdaptiveTabStripTabControl();
            var oldParent = tabControl.Parent as Grid;
            oldParent?.Children.Remove(tabControl);

            var parent = item.TabControl.FindAncestorOfType<AdaptiveTabStripTabControl>();
            parent?.Items.Remove(item.TabControl);

            tabControl.Items.Add(item.TabControl);
            Grid.SetColumn(tabControl, item.Column);
            _dropTargetGrid.Children.Add(tabControl);
        }
    }

    private void GenerateColumns(Grid grid, HashSet<int> items)
    {
        grid.ColumnDefinitions.Clear();
        grid.Children.Clear();

        var columnCount = items.ToArray().Length;

        for (var i = 0; i < columnCount; i++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
            if (i >= columnCount - 1)
            {
                continue;
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
            if (
                child is AdaptiveTabStripTabControl tabControl
                && Grid.GetColumn(tabControl) == columnIndex
            )
            {
                return tabControl;
            }
        }

        return null;
    }

    private void UnsplitAll()
    {
        foreach (var item in _shellItems)
        {
            item.Column = 0;
        }

        UpdateGrid();
    }

    private TabItem CreateTabItem(object content)
    {
        var header = new TabStripItem()
        {
            Content = content,
            ContentTemplate = TabControlStripItemTemplate,
        };
        SubscribeToEvents(header);
        var tab = new TabItem() { Content = content, Header = header };
        tab.Loaded += OnTabLoaded;
        return tab;
    }

    private void SubscribeToEvents(TabStripItem header)
    {
        header.PointerPressed += PressedHandler;
        header.PointerMoved += PointerMovedHandler;
    }

    private void OnTabUnloaded(object? sender, RoutedEventArgs e)
    {
        if (sender is TabItem { Header: TabStripItem header } tabItem)
        {
            header.PointerPressed -= PressedHandler;
            header.PointerMoved -= PointerMovedHandler;
            tabItem.Loaded += OnTabLoaded;
            tabItem.Unloaded -= OnTabUnloaded;
        }
    }

    private void OnTabLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is TabItem { Header: TabStripItem header } tabItem)
        {
            SubscribeToEvents(header);
            tabItem.Unloaded += OnTabUnloaded;
            tabItem.Loaded -= OnTabLoaded;
        }
    }

    #region HelperMethods

    private bool IsCursorWithinTargetBorder(Point cursorPosition, Border targetBorder)
    {
        return targetBorder.Bounds.Contains(cursorPosition);
    }

    private bool IsCursorWithinTabControl(
        Point cursorPosition,
        AdaptiveTabStripTabControl targetPanel
    )
    {
        return targetPanel.Bounds.Contains(cursorPosition);
    }

    private bool IsCursorOutOfDockControl(Point cursorPosition)
    {
        if (this.GetVisualRoot() is not Window window)
        {
            throw new Exception($"{nameof(window)} is null");
        }

        return window.Bounds.Contains(cursorPosition);
    }

    private void AddTabItemToTabControl(TabItem tabItem, Border selectorBorder)
    {
        var updateItem = _shellItems.Find(shellItem => shellItem.TabControl == tabItem);
        if (updateItem == null)
        {
            return;
        }

        if (selectorBorder == _leftSelector)
        {
            updateItem.Column -= ColumnIncrement;

            if (
                _shellItems.Count(shellItem => shellItem.Column.Equals(updateItem.Column))
                >= ColumnIncrement
            )
            {
                updateItem.Column -= ColumnIncrement;
            }

            if (updateItem.Column < 0)
            {
                foreach (var item in _shellItems)
                {
                    item.Column += ColumnIncrement;
                }

                updateItem.Column = 0;
            }
        }
        else if (selectorBorder == _rightSelector)
        {
            updateItem.Column += ColumnIncrement;

            if (
                _shellItems.Count(shellItem => shellItem.Column.Equals(updateItem.Column))
                >= ColumnIncrement
            )
            {
                updateItem.Column += ColumnIncrement;
            }
        }

        UpdateGrid();
    }

    private void SortShellItems()
    {
        if (_shellItems.Count == 0)
        {
            return;
        }

        var minItem = _shellItems.MinItem(shellItem => shellItem.Column);
        if (minItem.Column != 0)
        {
            foreach (var item in _shellItems)
            {
                item.Column -= ColumnIncrement;
            }
        }

        var maxColumnIndex = MaxSplitAmount * ColumnIncrement;
        var maxItem = _shellItems.MaxItem(shellItem => shellItem.Column);
        if (maxItem.Column >= maxColumnIndex)
        {
            maxItem.Column -= ColumnIncrement;
        }

        _shellItems = _shellItems.OrderBy(shellItem => shellItem.Column).ToList();

        var gapPairs = _shellItems
            .Zip(_shellItems.Skip(1), (prev, curr) => new { prev, curr })
            .Where(pair => pair.curr.Column - pair.prev.Column > 2)
            .ToList();
        foreach (var pair in gapPairs)
        {
            pair.curr.Column -= ColumnIncrement;
        }
    }

    #endregion

    #region Properties

    public static StyledProperty<ReactiveCommand> UnSplitAllCommandProperty =
        AvaloniaProperty.Register<DockControl, ReactiveCommand>(nameof(UnSplitAllCommand));

    public ReactiveCommand UnSplitAllCommand
    {
        get => GetValue(UnSplitAllCommandProperty);
        set => SetValue(UnSplitAllCommandProperty, value);
    }

    public static readonly StyledProperty<IDataTemplate?> TabControlStripItemTemplateProperty =
        AvaloniaProperty.Register<DockControl, IDataTemplate?>(nameof(TabControlStripItemTemplate));

    [InheritDataTypeFromItems("ItemsSource")]
    public IDataTemplate? TabControlStripItemTemplate
    {
        get => GetValue(TabControlStripItemTemplateProperty);
        set => SetValue(TabControlStripItemTemplateProperty, value);
    }

    public static readonly StyledProperty<int> MaxSplitAmountProperty = AvaloniaProperty.Register<
        DockControl,
        int
    >(nameof(MaxSplitAmount), 4);

    public int MaxSplitAmount
    {
        get => GetValue(MaxSplitAmountProperty);
        set => SetValue(MaxSplitAmountProperty, value);
    }

    public static readonly StyledProperty<IBrush> BorderHighLightColorProperty =
        AvaloniaProperty.Register<DockControl, IBrush>(
            nameof(BorderHighLightColor),
            Brushes.LightBlue
        );

    public IBrush BorderHighLightColor
    {
        get => GetValue(BorderHighLightColorProperty);
        set => SetValue(BorderHighLightColorProperty, value);
    }

    #endregion
}

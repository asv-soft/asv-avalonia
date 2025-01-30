using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Asv.Avalonia;

public enum DockOperation
{
    /// <summary>
    /// Fill dock.
    /// </summary>
    Fill,

    /// <summary>
    /// Dock to left.
    /// </summary>
    Left,

    /// <summary>
    /// Dock to bottom.
    /// </summary>
    Bottom,

    /// <summary>
    /// Dock to right.
    /// </summary>
    Right,

    /// <summary>
    /// Dock to top.
    /// </summary>
    Top,

    /// <summary>
    /// Dock to window.
    /// </summary>
    Window,
}

[Flags]
public enum DragAction
{
    /// <summary>
    /// No action.
    /// </summary>
    None = 0,

    /// <summary>
    /// Copy action.
    /// </summary>
    Copy = 1,

    /// <summary>
    /// Move action.
    /// </summary>
    Move = 2,

    /// <summary>
    /// Link action.
    /// </summary>
    Link = 4,
}

public class DockControl : SelectingItemsControl
{
    private TabItem? _selectedTab;

    private Point _dragStartPosition;

    // private Point _draggedPosition;
    private Border? _leftSelector;
    private Border? _rightSelector;

    //private Border? _topSelector;
    //private Border? _bottomSelector;
    private Border? _centerSelector;
    private TabControl? _leftIndicator;
    private TabControl? _rightIndicator;
    private Panel? _centerIndicator;
    private TabControl? _topIndicator;
    private TabControl? _bottomIndicator;

    // private bool _IsDragging;
    private readonly Dictionary<int, TabItem> _tabControls = new();
    private readonly List<Border> _targetBorders = new();


    public static readonly AttachedProperty<int> DockColumnProperty =
        AvaloniaProperty.RegisterAttached<DockControl, Control, int>("DockColumn", default);

    public static int GetDockColumn(Control control) => control.GetValue(DockColumnProperty);
    public static void SetDockColumn(Control control, int value) => control.SetValue(DockColumnProperty, value);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // _centralGrid = e.NameScope.Find<TabControl>("PART_DockCenterControlGrid") ?? throw new Exception();
        _topIndicator = e.NameScope.Find<TabControl>("PART_TopTabControl");
        _bottomIndicator = e.NameScope.Find<TabControl>("PART_BottomTabControl");
        _leftIndicator = e.NameScope.Find<TabControl>("PART_LeftTabControl");
        _rightIndicator = e.NameScope.Find<TabControl>("PART_RightTabControl");
        _centerIndicator = e.NameScope.Find<Panel>("PART_CenterIndicator");

        //_topSelector = e.NameScope.Find<Border>("PART_TopSelector");
        //_bottomSelector = e.NameScope.Find<Border>("PART_BottomSelector");
        _leftSelector = e.NameScope.Find<Border>("PART_LeftSelector");
        _rightSelector = e.NameScope.Find<Border>("PART_RightSelector");
        _centerSelector = e.NameScope.Find<Border>("PART_CenterSelector");

        //_targetBorders.Add(_topSelector!);
        //_targetBorders.Add(_bottomSelector!);
        _targetBorders.Add(_leftSelector!);
        _targetBorders.Add(_rightSelector!);
        _targetBorders.Add(_centerSelector!);

        UpdateTabs();
    }

    protected override void LogicalChildrenCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        base.LogicalChildrenCollectionChanged(sender, e);

        UpdateTabs();
    }

    private void UpdateTabs()
    {
        if (_leftIndicator == null)
        {
            return;
        }

        _leftIndicator.Items.Clear();
        _tabControls.Clear();

        if (Items == null)
        {
            return;
        }

        var groupedItems = Items.Cast<object>();

        foreach (var group in groupedItems)
        {
            _leftIndicator!.Items.Add(CreateTabItem(group));
        }
    }

    private bool IsMinimumDragDistance(Point diff)
    {
        return Math.Abs(diff.Y + Bounds.Width) > Bounds.Width
               || Math.Abs(diff.X + Bounds.Height) > Bounds.Height;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_selectedTab != null)
        {
            var pointerPosition = e.GetPosition(this);

            // Проверяем попадание мыши в границы PART_TopSelector
            foreach (var border in _targetBorders)
            {
                border.Background = IsCursorWithinTargetBorder(pointerPosition, border)
                    ? Brushes.LightBlue
                    : // Подсветить
                    Brushes.Transparent; // Сбросить подсветку
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
       //`
       //
       //.base.OnPointerReleased(e);
        foreach (var border in _targetBorders)
        {
            border.Background = Brushes.Transparent; // Сбросить подсветку
        }

        if (_selectedTab == null)
        {
            return;
        }
        
        var window = this.GetVisualRoot() as Window;
        if (window == null)
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
                var parent = _selectedTab.FindAncestorOfType<TabControl>();
                parent!.Items.Remove(_selectedTab);
                var win = new Window()
                {
                    Content = _selectedTab.Content,
                    
                    //Title = _selectedTab.Header!.ToString(),
                };
                win.Show();
            }
        }

        _selectedTab = null; // Сбрасываем перетаскиваемую вкладку
    }

    private bool IsTabSelected;

    private void PressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _dragStartPosition = e.GetPosition(this);

            var source = e.Source as Visual;
            var tab = source is TabItem item ? item : source?.FindAncestorOfType<TabItem>();
            if (tab != null)
            {
                IsTabSelected = true;
                _selectedTab = tab;
            }

            e.Pointer.Capture(this);
        }
    }

    private TabItem CreateTabItem(object content)
    {
        var header = new TabStripItem()
        {
            Content = (content as Control)?.Name ?? "Tab",
        };
        header.PointerPressed += PressedHandler;
        header.PointerMoved += PointerMovedHandler;
        var tab = new TabItem
        {
            Content = content,
            Header = header,
        };
        return tab;
    }

    private bool IsElementWithinBounds(Visual child, Visual parent)
    {
        if (child == null || parent == null)
        {
            return false;
        }

        // Получаем границы дочернего элемента
        var childBounds = child.Bounds;

        // Преобразуем координаты дочернего элемента в систему координат родительского
        var childPositionInParent = child.TranslatePoint(new Point(0, 0), parent);
        if (childPositionInParent == null)
        {
            return false;
        }

        var transformedChildBounds = new Rect(childPositionInParent.Value, childBounds.Size);

        // Получаем границы родительского элемента
        var parentBounds = parent.Bounds;

        // Проверяем, находится ли элемент в границах
        return parentBounds.Contains(transformedChildBounds.TopLeft) &&
               parentBounds.Contains(transformedChildBounds.BottomRight);
    }


    private void PointerMovedHandler(object? sender, PointerEventArgs e)
    {
        if (_selectedTab != null)
        {
            var pointerPosition = e.GetPosition(this);

            // Проверяем попадание мыши в границы PART_TopSelector
            foreach (var border in _targetBorders)
            {
                border.Background = IsCursorWithinTargetBorder(pointerPosition, border)
                    ? Brushes.LightBlue
                    : // Подсветить
                    Brushes.Transparent; // Сбросить подсветку
            }
        }
    }
    

    private bool IsCursorWithinTargetBorder(Point cursorPosition, Border targetBorder)
    {
        // Получаем границы TabControl относительно окна
        var tabControlBounds = targetBorder.Bounds;
        var tabControlPosition = targetBorder.TranslatePoint(new Point(0, 0), this);
        if (tabControlPosition == null)
        {
            return false;
        }

        // Проверяем, находится ли точка в пределах границ
        return targetBorder.Bounds.Contains(cursorPosition);
    }

    private bool IsCursorWithinDockControl(Point cursorPosition)
    {
        var window = this.GetVisualRoot() as Window;
        return window!.Bounds.Contains(cursorPosition);
    }

    private void AddTabItemToTabControl(TabItem tabItem, Border tabControl)
    {
        var parent = tabItem.FindAncestorOfType<TabControl>();
        parent!.Items.Remove(tabItem);

        if (tabControl == _leftSelector)
        {
            _leftIndicator!.Items.Add(tabItem);
        }
        else if (tabControl == _rightSelector)
        {
            _rightIndicator!.Items.Add(tabItem);
        }
        else if (tabControl == _centerSelector)
        {
        }
    }

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentControl.ContentTemplateProperty.AddOwner<TabControl>();

    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }
}
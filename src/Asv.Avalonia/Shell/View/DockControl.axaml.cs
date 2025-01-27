using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace Asv.Avalonia;

public class DockControl : SelectingItemsControl
{
    private Grid? _grid;
    private readonly Dictionary<int, TabControl> _tabControls = new();

    public static readonly AttachedProperty<int> DockColumnProperty =
        AvaloniaProperty.RegisterAttached<DockControl, Control, int>("DockColumn", default);

    public static int GetDockColumn(Control control) => control.GetValue(DockColumnProperty);

    public static void SetDockColumn(Control control, int value) =>
        control.SetValue(DockColumnProperty, value);

    public DockControl() { }

    protected override void OnInitialized()
    {
        base.OnInitialized();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _grid = e.NameScope.Find<Grid>("PART_DockControlGrid") ?? throw new Exception();
        _grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
        UpdateTabs();
    }

    protected override void LogicalChildrenCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e
    )
    {
        base.LogicalChildrenCollectionChanged(sender, e);
        UpdateTabs();
    }

    private void UpdateTabs()
    {
        if (_grid == null)
        {
            return;
        }

        _grid.Children.Clear();
        _tabControls.Clear();
        _grid.ColumnDefinitions.Clear();

        if (Items == null)
        {
            return;
        }

        // Группировка элементов по колонкам
        var groupedItems = Items
            .Cast<object>()
            .GroupBy(item =>
                GetDockColumn(
                    item as Control ?? throw new InvalidCastException("Items must be controls")
                )
            );

        foreach (var group in groupedItems.OrderBy(g => g.Key))
        {
            // Создаем колонку
            _grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            // Создаем TabControl для этой колонки
            var tabControl = new TabControl
            {
                ItemsSource = group.Select(CreateTabItem),
                ItemTemplate = ItemTemplate,
                ItemsPanel = ItemsPanel,
                ContentTemplate = ContentTemplate,
            };

            _tabControls[group.Key] = tabControl;

            _grid.Children.Add(tabControl);
            Grid.SetColumn(tabControl, group.Key);
        }
    }

    private TabItem CreateTabItem(object content)
    {
        return new TabItem { Content = content, Header = (content as Control)?.Name ?? "Tab" };
    }

    /*protected override Size MeasureOverride(Size availableSize)
    {
        _grid.Measure(availableSize);
        return _grid.DesiredSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        _grid.Arrange(new Rect(finalSize));
        return finalSize;
    }*/

    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty =
        ContentControl.ContentTemplateProperty.AddOwner<TabControl>();
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }
}

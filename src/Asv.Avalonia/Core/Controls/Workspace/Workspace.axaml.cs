using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;

namespace Asv.Avalonia;

public enum WorkspaceDock
{
    Left,
    Right,
    Bottom,
    Center,
}

public class Workspace : ItemsControl
{
    public static readonly StyledProperty<bool> IsLeftPaneOpenProperty =
        WorkspacePanel.IsLeftPaneOpenProperty.AddOwner<Workspace>();

    public static readonly StyledProperty<bool> IsRightPaneOpenProperty =
        WorkspacePanel.IsRightPaneOpenProperty.AddOwner<Workspace>();

    /// <summary>
    /// Gets or sets whether the left group of widgets is open.
    /// </summary>
    public bool IsLeftPaneOpen
    {
        get => GetValue(IsLeftPaneOpenProperty);
        set => SetValue(IsLeftPaneOpenProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the right group of widgets is open.
    /// </summary>
    public bool IsRightPaneOpen
    {
        get => GetValue(IsRightPaneOpenProperty);
        set => SetValue(IsRightPaneOpenProperty, value);
    }

    public static readonly FuncValueConverter<double, Thickness> SpacingToPaddingConverter =
        new(value => new Thickness(value));

    public static readonly StyledProperty<double> WidgetSpacingProperty =
        WorkspacePanel.WidgetSpacingProperty.AddOwner<Workspace>();

    /// <summary>
    /// Gets or sets the spacing between widgets and the size of the resize areas.
    /// </summary>
    public double WidgetSpacing
    {
        get => GetValue(WidgetSpacingProperty);
        set => SetValue(WidgetSpacingProperty, value);
    }

    protected override Control CreateContainerForItemOverride(
        object? item,
        int index,
        object? recycleKey
    )
    {
        return new WorkspaceItem();
    }

    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        return NeedsContainer<WorkspaceItem>(item, out recycleKey);
    }
}

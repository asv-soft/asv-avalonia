using Avalonia;
using Avalonia.Controls.Primitives;
using Material.Icons;

namespace Asv.Avalonia;

public partial class WorkspaceItem
{
    public static readonly StyledProperty<WorkspaceDock> PositionProperty =
        AvaloniaProperty.Register<WorkspaceItem, WorkspaceDock>(nameof(Position));

    public WorkspaceDock Position
    {
        get => GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public static readonly DirectProperty<WorkspaceItem, MaterialIconKind?> IconProperty =
        AvaloniaProperty.RegisterDirect<WorkspaceItem, MaterialIconKind?>(
            nameof(Icon),
            o => o.Icon,
            (o, v) => o.Icon = v
        );

    public MaterialIconKind? Icon
    {
        get;
        set => SetAndRaise(IconProperty, ref field, value);
    } = MaterialIconKind.ListBox;

    public static readonly StyledProperty<FlyoutBase?> FlyoutProperty = AvaloniaProperty.Register<
        WorkspaceItem,
        FlyoutBase?
    >(nameof(Flyout));

    public FlyoutBase? Flyout
    {
        get => GetValue(FlyoutProperty);
        set => SetValue(FlyoutProperty, value);
    }

    public static readonly StyledProperty<bool> IsExpandedProperty = AvaloniaProperty.Register<
        WorkspaceItem,
        bool
    >(nameof(IsExpanded), defaultValue: true);

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly StyledProperty<bool> CanExpandProperty = AvaloniaProperty.Register<
        WorkspaceItem,
        bool
    >(nameof(CanExpand), defaultValue: true);

    public bool CanExpand
    {
        get => GetValue(CanExpandProperty);
        set => SetValue(CanExpandProperty, value);
    }
}

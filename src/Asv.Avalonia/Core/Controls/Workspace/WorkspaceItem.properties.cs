using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public partial class WorkspaceItem
{
    public WorkspaceItem()
    {
        OpenContextMenu = new ReactiveCommand(_ => ContextMenu?.Open());
    }

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

    public static readonly StyledProperty<AsvColorKind> IconColorProperty =
        AvaloniaProperty.Register<WorkspaceItem, AsvColorKind>(nameof(IconColor));

    public AsvColorKind IconColor
    {
        get => GetValue(IconColorProperty);
        set => SetValue(IconColorProperty, value);
    }

    public static readonly StyledProperty<FlyoutBase?> FlyoutProperty =
        Button.FlyoutProperty.AddOwner<WorkspaceItem>();

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

    public static readonly DirectProperty<WorkspaceItem, ICommand?> OpenContextMenuProperty =
        AvaloniaProperty.RegisterDirect<WorkspaceItem, ICommand?>(
            nameof(OpenContextMenu),
            o => o.OpenContextMenu,
            (o, v) => o.OpenContextMenu = v
        );

    public ICommand? OpenContextMenu
    {
        get;
        set => SetAndRaise(OpenContextMenuProperty, ref field, value);
    }
}

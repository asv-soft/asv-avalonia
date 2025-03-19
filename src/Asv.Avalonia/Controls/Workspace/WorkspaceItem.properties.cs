using Avalonia;

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
}
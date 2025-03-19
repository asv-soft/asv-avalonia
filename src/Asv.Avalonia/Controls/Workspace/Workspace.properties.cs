using Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia;

public partial class Workspace
{
    public static readonly StyledProperty<WorkspaceDock> DockProperty = AvaloniaProperty.Register<
        Workspace,
        WorkspaceDock
    >(nameof(Dock), WorkspaceDock.Left);

    public WorkspaceDock Dock
    {
        get => GetValue(DockProperty);
        set => SetValue(DockProperty, value);
    }
}
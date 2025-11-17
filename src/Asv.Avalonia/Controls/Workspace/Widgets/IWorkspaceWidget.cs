using Material.Icons;

namespace Asv.Avalonia;

public interface IWorkspaceWidget : IRoutable
{
    MaterialIconKind? Icon { get; }
    string? Header { get; }
    WorkspaceDock Position { get; }
    bool IsExpanded { get; }
    bool CanExpand { get; }
    MenuTree? MenuView { get; }
    bool IsVisible { get; set; }
}

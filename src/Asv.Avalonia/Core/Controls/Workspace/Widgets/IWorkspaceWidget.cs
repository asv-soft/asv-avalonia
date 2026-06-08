using Material.Icons;

namespace Asv.Avalonia;

public interface IWorkspaceWidget : IViewModel
{
    MaterialIconKind? Icon { get; }
    AsvColorKind IconColor { get; }
    string? Header { get; }
    WorkspaceDock Position { get; }
    bool CanExpand { get; }
    bool IsExpanded { get; set; }
    bool IsVisible { get; set; }
    MenuTree? MenuView { get; }
}

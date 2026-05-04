using Avalonia.Media;
using Material.Icons;

namespace Asv.Avalonia;

public interface IWorkspaceWidget : IViewModel
{
    MaterialIconKind? Icon { get; }
    AsvColorKind IconColor { get; }
    string? Header { get; }
    WorkspaceDock Position { get; }
    bool IsExpanded { get; }
    bool CanExpand { get; }
    MenuTree? MenuView { get; }
    bool IsVisible { get; set; }
}

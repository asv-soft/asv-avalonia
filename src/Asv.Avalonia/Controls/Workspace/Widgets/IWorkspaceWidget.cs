using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;

namespace Asv.Avalonia;

public interface IWorkspaceWidget : IRoutable
{
    MaterialIconKind? Icon { get; }
    string Header { get; }
    WorkspaceDock Position { get; }
    bool IsExpanded { get; }
    bool CanExpand { get; }
    MenuTree? MenuView { get; }
}

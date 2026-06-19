using Avalonia.Controls;

namespace Asv.Avalonia.Example;

public partial class WorkspacePageView : UserControl
{
    private const string WorkspaceLayoutId = "WorkspacePage.Workspace";

    private readonly IDisposable _layout;

    public WorkspacePageView()
    {
        InitializeComponent();

        _layout = this.RegisterWorkspaceLayout(WorkspaceLayoutId, PART_Workspace);
        DetachedFromVisualTree += (_, _) => _layout.Dispose();
    }
}

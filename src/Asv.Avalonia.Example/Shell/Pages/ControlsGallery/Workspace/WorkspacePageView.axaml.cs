using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor<WorkspacePageViewModel>]
public partial class WorkspacePageView : UserControl
{
    public WorkspacePageView()
    {
        InitializeComponent();
    }
}

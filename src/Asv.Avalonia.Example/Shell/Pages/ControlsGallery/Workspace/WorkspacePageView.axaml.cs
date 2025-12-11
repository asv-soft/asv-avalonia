using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example;

[ExportViewFor<WorkspacePageViewModel>]
public partial class WorkspacePageView : UserControl
{
    public WorkspacePageView()
    {
        InitializeComponent();
    }
}

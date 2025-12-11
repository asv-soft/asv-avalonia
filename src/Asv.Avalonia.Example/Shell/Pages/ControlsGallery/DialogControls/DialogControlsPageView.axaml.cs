using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(DialogControlsPageViewModel))]
public partial class DialogControlsPageView : UserControl
{
    public DialogControlsPageView()
    {
        InitializeComponent();
    }
}

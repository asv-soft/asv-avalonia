using Avalonia.Controls;

namespace Asv.Avalonia.Example.Pages.Dialog;

[ExportViewFor(typeof(DialogBoardViewModel))]
public partial class DialogBoardView : UserControl
{
    public DialogBoardView()
    {
        InitializeComponent();
    }
}

using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(DialogBoardViewModel))]
public partial class DialogBoardView : UserControl
{
    public DialogBoardView()
    {
        InitializeComponent();
    }
}

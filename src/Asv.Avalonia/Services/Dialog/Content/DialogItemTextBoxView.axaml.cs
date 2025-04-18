using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(DialogItemTextBoxViewModel))]
public partial class DialogItemTextBoxView : UserControl
{
    public DialogItemTextBoxView()
    {
        InitializeComponent();
    }
}

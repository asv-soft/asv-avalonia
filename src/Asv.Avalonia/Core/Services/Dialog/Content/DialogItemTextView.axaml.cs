using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(DialogItemTextViewModel))]
public partial class DialogItemTextView : UserControl
{
    public DialogItemTextView()
    {
        InitializeComponent();
    }
}

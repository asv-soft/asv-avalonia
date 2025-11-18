using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(DialogItemUnsavedChangesViewModel))]
public partial class DialogItemUnsavedChangesView : UserControl
{
    public DialogItemUnsavedChangesView()
    {
        InitializeComponent();
    }
}

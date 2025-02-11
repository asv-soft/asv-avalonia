using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(DocumentViewModel))]
public partial class DocumentView : UserControl
{
    public DocumentView()
    {
        InitializeComponent();
    }
}

using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(DocumentPageViewModel))]
public partial class DocumentPageView : UserControl
{
    public DocumentPageView()
    {
        InitializeComponent();
    }
}

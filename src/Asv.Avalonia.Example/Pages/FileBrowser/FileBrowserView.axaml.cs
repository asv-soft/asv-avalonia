using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(FileBrowserViewModel))]
public partial class FileBrowserView : UserControl
{
    public FileBrowserView()
    {
        InitializeComponent();
    }
}

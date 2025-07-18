using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(LogViewerViewModel))]
public partial class LogViewerView : UserControl
{
    public LogViewerView()
    {
        InitializeComponent();
    }
}

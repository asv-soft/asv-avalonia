using Avalonia.Controls;

namespace Asv.Avalonia.IO;

[ExportViewFor(typeof(PortViewModel))]
public partial class PortView : UserControl
{
    public PortView()
    {
        InitializeComponent();
    }
}

using Avalonia.Controls;

namespace Asv.Avalonia.IO;

[ExportViewFor(typeof(PortViewModel))]
public partial class PortVIew : UserControl
{
    public PortVIew()
    {
        InitializeComponent();
    }
}

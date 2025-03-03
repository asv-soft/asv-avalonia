using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(UdpPortViewModel))]
public partial class UdpPortView : UserControl
{
    public UdpPortView()
    {
        InitializeComponent();
    }
}

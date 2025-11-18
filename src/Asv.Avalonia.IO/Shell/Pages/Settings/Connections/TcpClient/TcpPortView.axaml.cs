using Avalonia.Controls;

namespace Asv.Avalonia.IO;

[ExportViewFor(typeof(TcpPortViewModel))]
public partial class TcpPortView : UserControl
{
    public TcpPortView()
    {
        InitializeComponent();
    }
}

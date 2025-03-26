using Avalonia.Controls;

namespace Asv.Avalonia.IO;

[ExportViewFor(typeof(TcpClientPortViewModel))]
public partial class TcpClientPortView : UserControl
{
    public TcpClientPortView()
    {
        InitializeComponent();
    }
}

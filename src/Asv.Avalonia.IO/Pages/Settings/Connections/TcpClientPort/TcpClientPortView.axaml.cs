using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.IO;

[ExportViewFor(typeof(TcpClientPortViewModel))]
public partial class TcpClientPortView : UserControl
{
    public TcpClientPortView()
    {
        InitializeComponent();
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.IO;

[ExportViewFor(typeof(TcpServerPortViewModel))]
public partial class TcpServerPortView : UserControl
{
    public TcpServerPortView()
    {
        InitializeComponent();
    }
}

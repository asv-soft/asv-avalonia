using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(TcpPortViewModel))]
public partial class TcpPortView : UserControl
{
    public TcpPortView()
    {
        InitializeComponent();
    }
}

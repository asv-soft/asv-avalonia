using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.IO;

[ExportViewFor(typeof(UdpPortViewModel))]
public partial class UdpPortView : UserControl
{
    public UdpPortView()
    {
        InitializeComponent();
    }
}

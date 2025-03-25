using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.IO;

[ExportViewFor(typeof(PortViewModel))]
public partial class PortVIew : UserControl
{
    public PortVIew()
    {
        InitializeComponent();
    }
}

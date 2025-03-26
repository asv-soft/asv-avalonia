using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.IO;

[ExportViewFor(typeof(SerialPortViewModel))]
public partial class SerialPortView : UserControl
{
    public SerialPortView()
    {
        InitializeComponent();
    }
}

using Avalonia.Controls;

namespace Asv.Avalonia.IO;

[ExportViewFor(typeof(SerialPortViewModel))]
public partial class SerialPortView : UserControl
{
    public SerialPortView()
    {
        InitializeComponent();
    }
}

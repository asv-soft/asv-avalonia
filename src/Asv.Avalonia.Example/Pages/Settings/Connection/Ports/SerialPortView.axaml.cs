using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(SerialPortViewModel))]
public partial class SerialPortView : UserControl
{
    public SerialPortView()
    {
        InitializeComponent();
    }
}

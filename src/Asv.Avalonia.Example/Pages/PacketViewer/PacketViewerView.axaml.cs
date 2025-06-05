using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(PacketViewerViewModel))]
public partial class PacketViewerView : UserControl
{
    public PacketViewerView()
    {
        InitializeComponent();
    }
}

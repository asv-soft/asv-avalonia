using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(PacketViewerViewModel))]
public partial class PacketViewerView : UserControl
{
    public PacketViewerView()
    {
        InitializeComponent();
    }

    private void PacketList_LostFocus(object sender, RoutedEventArgs e)
    {
        if (DataContext is PacketViewerViewModel vm)
        {
            vm.SelectedPacket.Value = null;
        }
    }
}

using System.Linq;
using Asv.Avalonia.Example.PacketViewer;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(PacketViewerViewModel))]
public partial class PacketViewerView : UserControl
{
    public PacketViewerView()
    {
        InitializeComponent();
    }
}

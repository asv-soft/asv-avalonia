using Avalonia.Controls;

namespace Asv.Avalonia.Example.PacketViewer.Dialogs;

[ExportViewFor(typeof(SeparatorViewModel))]
public partial class SeparatorView : UserControl
{
    public SeparatorView()
    {
        InitializeComponent();
    }
}

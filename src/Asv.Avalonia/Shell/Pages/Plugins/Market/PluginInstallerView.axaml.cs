using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(PluginInstallerViewModel))]
public partial class PluginInstallerView : UserControl
{
    public PluginInstallerView()
    {
        InitializeComponent();
    }
}

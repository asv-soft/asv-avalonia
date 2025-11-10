using Avalonia.Controls;

namespace Asv.Avalonia.Plugins;

[ExportViewFor(typeof(PluginInstallerViewModel))]
public partial class PluginInstallerView : UserControl
{
    public PluginInstallerView()
    {
        InitializeComponent();
    }
}

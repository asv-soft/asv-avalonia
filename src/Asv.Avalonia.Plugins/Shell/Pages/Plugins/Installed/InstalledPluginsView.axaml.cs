using Avalonia.Controls;

namespace Asv.Avalonia.Plugins;

[ExportViewFor(typeof(InstalledPluginsViewModel))]
public partial class InstalledPluginsView : UserControl
{
    public InstalledPluginsView()
    {
        InitializeComponent();
    }
}

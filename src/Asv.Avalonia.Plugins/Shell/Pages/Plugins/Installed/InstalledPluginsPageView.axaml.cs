using Avalonia.Controls;

namespace Asv.Avalonia.Plugins;

[ExportViewFor(typeof(InstalledPluginsPageViewModel))]
public partial class InstalledPluginsPageView : UserControl
{
    public InstalledPluginsPageView()
    {
        InitializeComponent();
    }
}

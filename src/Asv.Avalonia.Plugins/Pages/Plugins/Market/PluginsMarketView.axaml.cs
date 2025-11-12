using Avalonia.Controls;

namespace Asv.Avalonia.Plugins;

[ExportViewFor(typeof(PluginsMarketViewModel))]
public partial class PluginsMarketView : UserControl
{
    public PluginsMarketView()
    {
        InitializeComponent();
    }
}

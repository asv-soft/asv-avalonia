using Avalonia.Controls;

namespace Asv.Avalonia.Plugins;

[ExportViewFor(typeof(PluginsMarketPageViewModel))]
public partial class PluginsMarketPageView : UserControl
{
    public PluginsMarketPageView()
    {
        InitializeComponent();
    }
}

using System.Composition;
using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(PluginsMarketViewModel))]
[ExportMetadata("Source", "System")]
public partial class PluginsMarketView : UserControl
{
    public PluginsMarketView()
    {
        InitializeComponent();
    }
}

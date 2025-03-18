using System.Composition;
using Avalonia.Controls;

namespace Asv.Avalonia.Plugins;

[ExportViewFor(typeof(PluginsSourcesViewModel))]
[ExportMetadata("Source", "System")]
public partial class PluginsSourcesView : UserControl
{
    public PluginsSourcesView()
    {
        InitializeComponent();
    }
}

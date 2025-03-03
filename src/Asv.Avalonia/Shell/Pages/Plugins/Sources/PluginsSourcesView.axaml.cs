using System.Composition;
using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(PluginsSourcesViewModel))]
[ExportMetadata("Source", "System")]
public partial class PluginsSourcesView : UserControl
{
    public PluginsSourcesView()
    {
        InitializeComponent();
    }
}

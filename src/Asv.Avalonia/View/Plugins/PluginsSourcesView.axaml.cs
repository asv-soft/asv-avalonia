using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(PluginsSourcesViewModel))]
public partial class PluginsSourcesView : UserControl
{
    public PluginsSourcesView()
    {
        InitializeComponent();
    }
}

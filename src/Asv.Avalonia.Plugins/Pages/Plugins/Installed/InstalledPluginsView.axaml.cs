using System.Composition;
using Avalonia.Controls;

namespace Asv.Avalonia.Plugins;

[ExportViewFor(typeof(InstalledPluginsViewModel))]
[ExportMetadata("Source", "System")]
public partial class InstalledPluginsView : UserControl
{
    public InstalledPluginsView()
    {
        InitializeComponent();
    }
}

using System.Composition;
using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(InstalledPluginsViewModel))]
[ExportMetadata("Source", "System")]
public partial class InstalledPluginsView : UserControl
{
    public InstalledPluginsView()
    {
        InitializeComponent();
    }
}

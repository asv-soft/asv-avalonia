using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(InstalledPluginsViewModel))]
public partial class InstalledPluginsView : UserControl
{
    public InstalledPluginsView()
    {
        InitializeComponent();
    }
}

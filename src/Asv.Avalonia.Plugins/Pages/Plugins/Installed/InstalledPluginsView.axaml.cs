using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Plugins;

[ExportViewFor(typeof(InstalledPluginsViewModel))]
public partial class InstalledPluginsView : UserControl
{
    public InstalledPluginsView()
    {
        InitializeComponent();
    }
}

using System.Composition;
using Avalonia.Controls;

namespace Asv.Avalonia.Plugins;

[ExportViewFor(typeof(SettingsPluginsSourcesViewModel))]
public partial class SettingsPluginsSourcesView : UserControl
{
    public SettingsPluginsSourcesView()
    {
        InitializeComponent();
    }
}

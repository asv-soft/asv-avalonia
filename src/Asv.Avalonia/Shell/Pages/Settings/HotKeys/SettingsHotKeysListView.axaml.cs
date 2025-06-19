using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(SettingsHotKeysListViewModel))]
public partial class SettingsHotKeysListView : UserControl
{
    public SettingsHotKeysListView()
    {
        InitializeComponent();
    }
}

using Avalonia;
using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(SettingsCommandListViewModel))]
public partial class SettingsCommandListView : UserControl
{
    public SettingsCommandListView()
    {
        InitializeComponent();
    }
}

using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(SettingsConnectionViewModel))]
public partial class SettingsConnectionView : UserControl
{
    public SettingsConnectionView()
    {
        InitializeComponent();
    }
}

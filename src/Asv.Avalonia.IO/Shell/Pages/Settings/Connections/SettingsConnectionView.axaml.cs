using Avalonia.Controls;

namespace Asv.Avalonia.IO;

[ExportViewFor(typeof(SettingsConnectionViewModel))]
public partial class SettingsConnectionView : UserControl
{
    public SettingsConnectionView()
    {
        InitializeComponent();
    }
}

using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(SettingsAppearanceViewModel))]
public partial class SettingsAppearanceView : UserControl
{
    public SettingsAppearanceView()
    {
        InitializeComponent();
    }
}

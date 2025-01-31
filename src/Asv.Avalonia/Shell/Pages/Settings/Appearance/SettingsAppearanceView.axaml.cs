using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

[ExportViewFor(typeof(SettingsAppearanceViewModel))]
public partial class SettingsAppearanceView : UserControl
{
    public SettingsAppearanceView()
    {
        InitializeComponent();
    }
}
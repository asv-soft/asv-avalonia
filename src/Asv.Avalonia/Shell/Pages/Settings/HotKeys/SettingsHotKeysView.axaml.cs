using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

[ExportViewFor(typeof(SettingsHotKeysViewModel))]
public partial class SettingsHotKeysView : UserControl
{
    public SettingsHotKeysView()
    {
        InitializeComponent();
    }
}

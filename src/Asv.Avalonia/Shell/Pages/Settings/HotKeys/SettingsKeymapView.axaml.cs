using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

[ExportViewFor(typeof(SettingsKeymapViewModel))]
public partial class SettingsKeymapView : UserControl
{
    public SettingsKeymapView()
    {
        InitializeComponent();
    }
}

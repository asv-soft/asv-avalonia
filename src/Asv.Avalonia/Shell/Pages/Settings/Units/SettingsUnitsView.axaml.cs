using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

[ExportViewFor(typeof(SettingsUnitsViewModel))]
public partial class SettingsUnitsView : UserControl
{
    public SettingsUnitsView()
    {
        InitializeComponent();
    }
}

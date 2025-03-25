using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.IO;

[ExportViewFor(typeof(SettingsConnectionViewModel))]
public partial class SettingsConnectionView : UserControl
{
    public SettingsConnectionView()
    {
        InitializeComponent();
    }
}

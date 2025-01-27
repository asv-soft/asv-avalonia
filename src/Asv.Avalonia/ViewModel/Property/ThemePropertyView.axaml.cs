using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

[ExportViewFor(typeof(ThemeProperty))]
public partial class ThemePropertyView : UserControl
{
    public ThemePropertyView()
    {
        InitializeComponent();
    }
}

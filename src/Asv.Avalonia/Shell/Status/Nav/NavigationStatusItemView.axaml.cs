using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

[ExportViewFor(typeof(NavigationStatusItemViewModel))]
public partial class NavigationStatusItemView : UserControl
{
    public NavigationStatusItemView()
    {
        InitializeComponent();
    }
}

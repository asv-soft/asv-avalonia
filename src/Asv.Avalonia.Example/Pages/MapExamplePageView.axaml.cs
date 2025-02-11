using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(MapExamplePageViewModel))]
public partial class MapExamplePageView : UserControl
{
    public MapExamplePageView()
    {
        InitializeComponent();
    }
}

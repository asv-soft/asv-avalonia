using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(MapExamplePageViewModel))]
public partial class MapExamplePageView : UserControl
{
    public MapExamplePageView()
    {
        InitializeComponent();
    }
}

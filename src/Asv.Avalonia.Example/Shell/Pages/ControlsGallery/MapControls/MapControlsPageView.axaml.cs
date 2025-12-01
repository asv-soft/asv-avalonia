using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(MapControlsPageViewModel))]
public partial class MapControlsPageView : UserControl
{
    public MapControlsPageView()
    {
        InitializeComponent();
    }
}

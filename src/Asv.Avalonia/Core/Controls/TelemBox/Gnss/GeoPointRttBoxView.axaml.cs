using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(GeoPointRttBoxViewModel))]
public partial class GeoPointRttBoxView : UserControl
{
    public GeoPointRttBoxView()
    {
        InitializeComponent();
    }
}

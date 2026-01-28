using Avalonia.Controls;

namespace Asv.Avalonia.GeoMap;

[ExportViewFor(typeof(GeoPointDialogViewModel))]
public partial class GeoPointDialogView : UserControl
{
    public GeoPointDialogView()
    {
        InitializeComponent();
    }
}

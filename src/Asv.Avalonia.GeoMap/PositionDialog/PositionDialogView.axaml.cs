using Avalonia.Controls;

namespace Asv.Avalonia.GeoMap;

[ExportViewFor(typeof(PositionDialogViewModel))]
public partial class PositionDialogView : UserControl
{
    public PositionDialogView()
    {
        InitializeComponent();
    }
}

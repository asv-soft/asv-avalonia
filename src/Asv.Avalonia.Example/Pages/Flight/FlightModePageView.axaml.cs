using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(FlightModePageViewModel))]
public partial class FlightModePageView : UserControl
{
    public FlightModePageView()
    {
        InitializeComponent();
    }
}

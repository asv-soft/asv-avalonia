using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(FlightPageViewModel))]
public partial class FlightPageView : UserControl
{
    public FlightPageView()
    {
        InitializeComponent();
    }
}

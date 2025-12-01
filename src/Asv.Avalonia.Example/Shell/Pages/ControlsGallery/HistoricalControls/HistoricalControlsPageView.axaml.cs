using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(HistoricalControlsPageViewModel))]
public partial class HistoricalControlsPageView : UserControl
{
    public HistoricalControlsPageView()
    {
        InitializeComponent();
    }
}

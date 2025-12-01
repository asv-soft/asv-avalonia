using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(InfoBoxControlsPageViewModel))]
public partial class InfoBoxControlsPageView : UserControl
{
    public InfoBoxControlsPageView()
    {
        InitializeComponent();
    }
}

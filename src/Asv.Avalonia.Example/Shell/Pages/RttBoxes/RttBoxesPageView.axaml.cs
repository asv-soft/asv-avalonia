using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(RttBoxesPageViewModel))]
public partial class RttBoxesPageView : UserControl
{
    public RttBoxesPageView()
    {
        InitializeComponent();
    }
}

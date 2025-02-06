using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(TestUnitsPageViewModel))]
public partial class TestUnitsPageView : UserControl
{
    public TestUnitsPageView()
    {
        InitializeComponent();
    }
}

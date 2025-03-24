using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(TestInfoBoxPageViewModel))]
public partial class TestInfoBoxPageView : UserControl
{
    public TestInfoBoxPageView()
    {
        InitializeComponent();
    }
}

using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(TestRestrictionsPageViewModel))]
public partial class TestRestrictionsPageView : UserControl
{
    public TestRestrictionsPageView()
    {
        InitializeComponent();
    }
}

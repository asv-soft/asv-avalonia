using System.Composition;
using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(HomePageViewModel))]
[ExportMetadata("Source", "System")]
public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        InitializeComponent();
    }
}

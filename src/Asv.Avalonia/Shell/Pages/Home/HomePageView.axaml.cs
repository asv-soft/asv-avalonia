using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

[ExportViewFor(typeof(HomePageViewModel))]
public partial class HomePageView : UserControl
{
    public HomePageView()
    {
        InitializeComponent();
    }
}
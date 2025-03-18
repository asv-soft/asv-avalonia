using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(MavParamsPageViewModelViewModel))]
public partial class MavParamsPageView : UserControl
{
    public MavParamsPageView()
    {
        InitializeComponent();
    }
}

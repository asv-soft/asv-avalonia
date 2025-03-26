using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(MavParamsPageViewModelViewModel))]
public partial class MavParamsPageView : UserControl
{
    public MavParamsPageView()
    {
        InitializeComponent();
    }
}

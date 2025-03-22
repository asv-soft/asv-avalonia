using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(MavParamsPageViewModel))]
public partial class MavParamsPageView : UserControl
{
    public MavParamsPageView()
    {
        InitializeComponent();
    }
}

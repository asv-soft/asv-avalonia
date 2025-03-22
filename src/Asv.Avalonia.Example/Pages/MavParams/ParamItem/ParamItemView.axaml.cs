using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(ParamItemViewModel))]
public partial class ParamItemView : UserControl
{
    public ParamItemView()
    {
        InitializeComponent();
    }
}

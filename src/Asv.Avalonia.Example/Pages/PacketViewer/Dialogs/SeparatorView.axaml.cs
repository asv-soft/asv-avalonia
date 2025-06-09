using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(SeparatorViewModel))]
public partial class SeparatorView : UserControl
{
    public SeparatorView()
    {
        InitializeComponent();
    }
}

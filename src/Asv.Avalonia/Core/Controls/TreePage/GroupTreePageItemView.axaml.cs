using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(GroupTreePageItemViewModel))]
public partial class GroupTreePageItemView : UserControl
{
    public GroupTreePageItemView()
    {
        InitializeComponent();
    }
}

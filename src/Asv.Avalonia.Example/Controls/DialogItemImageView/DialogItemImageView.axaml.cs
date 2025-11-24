using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(DialogItemImageViewModel))]
public partial class DialogItemImageView : UserControl
{
    public DialogItemImageView()
    {
        InitializeComponent();
    }
}

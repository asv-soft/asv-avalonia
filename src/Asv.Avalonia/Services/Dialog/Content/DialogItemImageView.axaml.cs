using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(DialogItemImageViewModel))]
public partial class DialogItemImageView : UserControl
{
    public DialogItemImageView()
    {
        InitializeComponent();
    }
}

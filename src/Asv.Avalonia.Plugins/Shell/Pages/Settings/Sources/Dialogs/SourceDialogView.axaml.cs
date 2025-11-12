using Avalonia.Controls;

namespace Asv.Avalonia.Plugins;

[ExportViewFor(typeof(SourceDialogViewModel))]
public partial class SourceDialogView : UserControl
{
    public SourceDialogView()
    {
        InitializeComponent();
    }
}

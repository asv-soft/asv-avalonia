using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(BurstDownloadDialogViewModel))]
public partial class BurstDownloadDialogView : UserControl
{
    public BurstDownloadDialogView()
    {
        InitializeComponent();
    }
}

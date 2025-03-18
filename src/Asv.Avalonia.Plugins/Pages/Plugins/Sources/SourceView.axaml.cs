using Avalonia.Controls;

namespace Asv.Avalonia.Plugins;

[ExportViewFor(typeof(SourceViewModel))]
public partial class SourceView : UserControl
{
    public SourceView()
    {
        InitializeComponent();
    }
}

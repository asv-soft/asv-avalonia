using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

[ExportViewFor(typeof(SourceViewModel))]
public partial class SourceView : UserControl
{
    public SourceView()
    {
        InitializeComponent();
    }
}

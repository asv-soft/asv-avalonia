using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(PropertyEditorPageViewModel))]
public partial class PropertyEditorPageView : UserControl
{
    public PropertyEditorPageView()
    {
        InitializeComponent();
    }
}

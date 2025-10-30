using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(PropertyEditorPageViewModel))]
public partial class PropertyEditorPageView : UserControl
{
    public PropertyEditorPageView()
    {
        InitializeComponent();
    }
}

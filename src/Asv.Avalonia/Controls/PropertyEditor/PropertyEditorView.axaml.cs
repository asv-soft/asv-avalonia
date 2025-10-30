using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

[ExportViewFor<PropertyEditorViewModel>]
public partial class PropertyEditorView : UserControl
{
    public PropertyEditorView()
    {
        InitializeComponent();
    }
}

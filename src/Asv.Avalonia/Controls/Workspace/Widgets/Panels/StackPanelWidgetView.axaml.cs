using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

[ExportViewFor<StackPanelWidgetViewModel>]
public partial class StackPanelWidgetView : UserControl
{
    public StackPanelWidgetView()
    {
        InitializeComponent();
    }
}

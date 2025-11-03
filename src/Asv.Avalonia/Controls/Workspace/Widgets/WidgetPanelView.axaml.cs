using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

[ExportViewFor<WidgetPanelViewModel>]
public partial class WidgetPanelView : UserControl
{
    public WidgetPanelView()
    {
        InitializeComponent();
    }
}
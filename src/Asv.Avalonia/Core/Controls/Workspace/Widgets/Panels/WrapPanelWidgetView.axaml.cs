using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

[ExportViewFor<WrapPanelWidgetViewModel>]
public partial class WrapPanelWidgetView : UserControl
{
    public WrapPanelWidgetView()
    {
        InitializeComponent();
    }
}

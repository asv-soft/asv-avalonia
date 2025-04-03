using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(UavWidgetViewModel))]
public partial class UavWidgetView : UserControl
{
    public UavWidgetView()
    {
        InitializeComponent();
    }
}

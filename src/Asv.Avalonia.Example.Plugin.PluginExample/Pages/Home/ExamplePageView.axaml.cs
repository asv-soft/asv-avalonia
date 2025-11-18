using Avalonia.Controls;

namespace Asv.Avalonia.Example.Plugin.PluginExample;

[ExportViewFor(typeof(ExamplePageViewModel))]
public partial class ExamplePageView : UserControl
{
    public ExamplePageView()
    {
        InitializeComponent();
    }
}

using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(BatteryRttItemViewModel))]
public partial class BatteryRttItem : UserControl
{
    public BatteryRttItem()
    {
        InitializeComponent();
    }
}
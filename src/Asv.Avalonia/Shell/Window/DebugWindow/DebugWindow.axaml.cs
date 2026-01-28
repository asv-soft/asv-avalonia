using Avalonia.Controls;

namespace Asv.Avalonia;

[ExportViewFor(typeof(DebugWindowViewModel))]
public partial class DebugWindow : Window
{
    public DebugWindow()
    {
        InitializeComponent();
    }
}

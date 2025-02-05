using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

[ExportViewFor(typeof(DebugWindowViewModel))]
public partial class DebugWindow : Window
{
    public DebugWindow()
    {
        InitializeComponent();
    }
}

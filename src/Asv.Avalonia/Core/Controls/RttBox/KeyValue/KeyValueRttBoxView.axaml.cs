using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

[ExportViewFor<KeyValueRttBoxViewModel>]
public partial class KeyValueRttBoxView : UserControl
{
    public KeyValueRttBoxView()
    {
        InitializeComponent();
    }
}

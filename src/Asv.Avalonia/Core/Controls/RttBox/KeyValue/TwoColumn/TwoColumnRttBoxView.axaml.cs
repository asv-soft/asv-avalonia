using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

[ExportViewFor<TwoColumnRttBoxViewModel>]
public partial class TwoColumnRttBoxView : UserControl
{
    public TwoColumnRttBoxView()
    {
        InitializeComponent();
    }
}

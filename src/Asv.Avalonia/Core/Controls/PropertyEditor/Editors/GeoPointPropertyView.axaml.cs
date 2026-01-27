using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Asv.Avalonia;

[ExportViewFor<GeoPointPropertyViewModel>]
public partial class GeoPointPropertyView : UserControl
{
    public GeoPointPropertyView()
    {
        InitializeComponent();
    }
}

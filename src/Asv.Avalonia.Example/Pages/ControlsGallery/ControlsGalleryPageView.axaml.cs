using Avalonia.Controls;

namespace Asv.Avalonia.Example;

[ExportViewFor(typeof(ControlsGalleryPageViewModel))]
public partial class ControlsGalleryPageView : UserControl
{
    public ControlsGalleryPageView()
    {
        InitializeComponent();
    }
}

using Avalonia.Controls;
using Avalonia.Input;

namespace Asv.Avalonia.GeoMap;

public partial class MapPreloadDialogView : UserControl
{
    public MapPreloadDialogView()
    {
        InitializeComponent();
    }

    private void MapClicked(object? sender, MapClickEventArgs args)
    {
        if (args.Button == MouseButton.Left && DataContext is MapPreloadDialogViewModel viewModel)
        {
            viewModel.SelectCenter(args.Point);
        }
    }
}

using Avalonia.Controls;
using Avalonia.Threading;

namespace Asv.Avalonia.GeoMap;

public partial class TileProviderSelectorView : UserControl
{
    public TileProviderSelectorView()
    {
        InitializeComponent();
    }

    private void OnDropDownOpened(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.Post(
            () =>
            {
                if (ProvidersComboBox.SelectedItem is { } selected)
                {
                    ProvidersComboBox.ScrollIntoView(selected);
                }
            },
            DispatcherPriority.Background
        );
    }
}

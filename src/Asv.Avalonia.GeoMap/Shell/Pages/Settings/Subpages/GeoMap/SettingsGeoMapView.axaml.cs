using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace Asv.Avalonia.GeoMap;

public partial class SettingsGeoMapView : UserControl
{
    public SettingsGeoMapView()
    {
        InitializeComponent();
    }

    private void OnProvidersListBoxLoaded(object? sender, RoutedEventArgs e)
    {
        if (ProvidersListBox.DataContext is not MapProviderProperty vm)
        {
            return;
        }

        Dispatcher.UIThread.Post(
            () =>
            {
                var target = vm.CurrentProvider.Value;
                if (target is not null)
                {
                    ProvidersListBox.ScrollIntoView(target);
                }
            },
            DispatcherPriority.Background
        );
    }
}

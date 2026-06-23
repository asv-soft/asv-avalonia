using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Asv.Avalonia.GeoMap;

public partial class MapView : UserControl
{
    public MapView()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (!Design.IsDesignMode && DataContext is IMap map)
        {
            map.Interaction.AttachMap(PART_Map, map.Anchors);
        }
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        if (!Design.IsDesignMode && DataContext is IMap map)
        {
            map.Interaction.DetachMap();
        }

        base.OnUnloaded(e);
    }
}

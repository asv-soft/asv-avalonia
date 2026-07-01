using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Asv.Avalonia.GeoMap;

public partial class MapView : UserControl
{
    public MapView()
    {
        InitializeComponent();

        AddHandler(ContextRequestedEvent, OnContextRequested, RoutingStrategies.Bubble);
    }

    private void OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (e.Handled || !e.TryGetPosition(this, out var position))
        {
            return;
        }

        var mapItem = this.GetVisualsAt(position)
            .Select(visual => visual.FindAncestorOfType<MapItem>(includeSelf: true))
            .FirstOrDefault(item => item is not null);

        if (
            mapItem?.ContextMenu is not { } contextMenu
            || (mapItem.DataContext as IMapAnchor)?.Menu is not { Count: > 0 }
        )
        {
            return;
        }

        contextMenu.Open(mapItem);
        e.Handled = true;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (!Design.IsDesignMode && DataContext is IMap map)
        {
            map.Interaction.AttachMap(PART_Map);
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

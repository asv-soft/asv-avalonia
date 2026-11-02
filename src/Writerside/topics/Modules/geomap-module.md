# GeoMap module

<img src="guide-geomap.png" dark-src="guide-geomap.png" alt="Map test with annotations and provider selector" thumbnail="true"/>

Open Home → Map test. The example exercises annotations, icon rotation, offsets, and the provider selector. Basemap availability depends on the configured provider and network access.

`Asv.Avalonia.GeoMap` provides tile loading, map controls, geographic markers, map settings, and coordinate dialogs. Register it after the framework core and shell:

```c#
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;

builder.RegisterModuleGeoMap();
```

The default registration includes `IMapService`, `ITileProviderService`, `ITileLoader`, `IZoomService`, memory and file-system tile caches, provider view models, dialogs, and the map settings page.

## Display a map with a marker

Use `MapViewModel` as the data context of `MapView`. The following factory can be called by a page view model that receives `IMapService` through dependency injection:

```c#
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Common;
using Material.Icons;

static MapViewModel CreateMap(IMapService mapService)
{
    var location = new GeoPoint(56.84, 60.61, 250);
    var map = new MapViewModel("site-map", mapService);
    map.CenterMap.Value = location;
    map.Zoom.Value = 12;
    map.Anchors.Add(new MapAnchor("inspection-site")
    {
        Header = "Inspection site",
        Location = location,
        Icon = MaterialIconKind.MapMarker,
    });
    return map;
}
```

Here, `GeoPoint` contains latitude and longitude in degrees and altitude in meters. Use stable, distinct model identifiers for your map and anchors. Dispose the map with its owning page; the map owns its anchor collection and disposes removed anchors. Change bound collections and properties on the UI thread.

If your page exposes the resulting model as `Map`, embed the view in its AXAML:

```xml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:map="clr-namespace:Asv.Avalonia.GeoMap;assembly=Asv.Avalonia.GeoMap">
    <map:MapView DataContext="{Binding Map}" />
</UserControl>
```

Adapt the surrounding control declaration and `x:DataType` to your page. `MapView` binds the center, zoom, rotation, provider, selected anchor, and marker collection. For lower-level composition, use `MapItemsControl`; the implementation of `MapView.axaml` is a working example of its bindings and item templates.

The example application includes **Map test** and a map-controls gallery. Their source is under `Asv.Avalonia.Example/Shell/Pages/MapTestPage` and `ControlsGallery/MapControls`.

## Providers and API keys

The default provider registry includes OpenStreetMap variants, Google, Bing, Yandex, ArcGIS, HERE, Thunderforest, WikiMapia, and an empty provider. This list describes registered implementations; it does not guarantee that every remote service is available or usable without credentials.

Use the map settings page to select a provider and edit credentials for protected providers. `IProtectedTileProvider.ApiKey` is the credential boundary; changing a protected HTTP provider's key resets its error state. `TileProviderSelectorViewModel` supplies the provider-selection UI.

To add your own implementation of `ITileProvider` after module registration:

```c#
builder.ViewModel.RegisterTileProvider<MyTileProvider>();
```

`MyTileProvider` is an application-defined class implementing `ITileProvider`. The registration adds it as a singleton. Inspect `HttpTileProvider`, `ProtectedHttpTileProvider`, and the providers under `Core/Services/ProviderService/Providers` before implementing request URLs and projections.

Tile loading uses memory and file-system caches. A cached tile is not evidence that new network requests succeed. Provider availability, attribution, licensing, quotas, and key requirements remain properties of the selected service; configure them for your deployment.

## Edit a geographic location

The module registers dialogs with `IDialogService`. A page that already receives that service can request a coordinate selection:

```c#
using Asv.Avalonia;
using Asv.Avalonia.GeoMap;
using Asv.Common;

GeoPoint? selected = await dialogs.ShowGeoPointDialogAsync(
    new GeoPoint(56.84, 60.61, 250));

if (selected is { } location)
{
    Map.CenterMap.Value = location;
}
```

The nullable result represents cancellation. For a credential editor, use `ShowEditApiKeyDialogAsync(currentApiKey)` and apply the accepted value to your provider/configuration. Opening the dialog alone does not persist an API key.

See `GeoMapDialogServiceExtensions.cs` for optional icon, status, color, and dialog-size arguments.

## Altitude display layer

`MapView` provides an optional polygon altitude layer, disabled by default.
Standalone anchor markers do not produce altitude geometry.

```c#
map.ShowAltitudes.Value = true;
map.AltitudePixelsPerMeter.Value = 0.3;
```

Each polygon vertex produces a screen-vertical stem from its geographic position.
Upper endpoints are connected in vertex order. Adjacent stems form filled side
faces. Closed polygons include the final edge back to the first vertex; open
polylines do not. There are no upper endpoint circles.

Outlines use the polygon pen at 50% opacity. Side faces use the same pen brush
at 15% opacity. The layer stroke is a fallback for polygons without a pen.
Map rotation changes base positions but stems always point upwards on screen.

Stem length is `abs(GeoPoint.Altitude) * AltitudePixelsPerMeter`, in
device-independent pixels. This is a visual scale independent of map zoom,
not terrain clearance or a conversion between altitude datums. Non-finite
heights and their adjacent faces are omitted. Map dragging preserves altitude.

Turning the layer off disconnects map, marker and polygon subscriptions and clears
tracked containers. `ShowPolygonAltitudes` remains an optional programmatic filter
and defaults to `true`; the example exposes a single altitude-layer switch.

Use `MapAltitudeToggle` for a round map-toolbar button matching `MapRuler`:

```xml
<map:MapAltitudeToggle
    IsChecked="{Binding MapViewModel.ShowAltitudes.Value, Mode=TwoWay}" />
```

## Map input presets

Open **Settings → Map** to choose a complete gesture for each map action.
Assignments are shared by all maps using the registered `IMapService`, take effect
immediately, and are saved in application configuration.

| Action | Default gesture |
|---|---|
| Pan the map | Left-button drag |
| Move editable markers | Ctrl + left-button drag |
| Rectangle selection | Shift + left-button drag |
| Zoom | Mouse wheel |

For dragging, choose the left or middle mouse button, optionally combined with
Ctrl, Shift, Alt, or Command/Windows. Zoom additionally offers the mouse wheel
with the same modifier choices. Selecting a drag gesture for zoom replaces wheel
zoom: drag upwards to zoom in and downwards to zoom out. Each 40 device-independent
pixels changes the zoom by one level. Trackpad pinch is controlled separately by
`EnableTouchpadGestures` and remains available even when no zoom preset is assigned.

An exact gesture cannot be assigned to two actions. The editor rejects the change,
shows which action already uses that gesture, and keeps the previous assignment.
For example, to use **Shift + left-button drag** for zoom, first change rectangle
selection to **Alt + left-button drag** or **Unassigned**, then assign Shift to zoom.
Use **Reset map controls** to restore all four defaults. Escape continues to cancel
an active map interaction session and is not changed by these pointer presets.

Read-only markers remain fixed. A marker's `CanDragWithoutModifier` opt-in is
preserved, except when the marker-move action is unassigned. Changing input
settings during a drag releases pointer capture and stops the old gesture.

Applications can also configure the shared service directly:

```c#
IMapInputService input = mapService.Input!;
if (!input.TryAssign(MapInputAction.MoveMarkers, MapGesturePreset.AltLeftDrag, out var conflict))
{
    // The previous assignment remains active; inspect conflict for the other action.
}
input.Reset();
```

A custom `IMapService` can omit `Input` and retain the default gestures. For a
standalone `MapItemsControl`, assign its `InputBindings` property directly.

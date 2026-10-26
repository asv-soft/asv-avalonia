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

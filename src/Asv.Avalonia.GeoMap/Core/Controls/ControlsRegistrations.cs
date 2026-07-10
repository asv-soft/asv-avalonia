using Asv.Avalonia;

namespace Asv.Avalonia.GeoMap;

public static class ControlsRegistrations
{
    public static CoreRegistrations.Builder RegisterControls(this CoreRegistrations.Builder builder)
    {
        builder
            .AppBuilder.ViewLocator.RegisterViewFor<MapViewModel, MapView>()
            .RegisterViewFor<MapWidget, MapWidgetView>()
            .RegisterViewFor<PropertyGeoPointViewModel, PropertyGeoPointView>();

        return builder;
    }
}

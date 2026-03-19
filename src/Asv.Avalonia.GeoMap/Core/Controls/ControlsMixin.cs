namespace Asv.Avalonia.GeoMap;

public static class ControlsMixin
{
    public static GeoMapMixin.Builder AddControls(this GeoMapMixin.Builder builder)
    {
        builder
            .Parent.ViewLocator.RegisterViewFor<MapViewModel, MapView>()
            .RegisterViewFor<MapWidget, MapWidgetView>();

        return builder;
    }
}

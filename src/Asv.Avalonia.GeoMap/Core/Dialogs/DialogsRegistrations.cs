using Asv.Avalonia;

namespace Asv.Avalonia.GeoMap;

public static class DialogsRegistrations
{
    public static CoreRegistrations.Builder RegisterDialogs(this CoreRegistrations.Builder builder)
    {
        builder.AppBuilder.Dialogs.RegisterPrefab<MapPreloadDialogPrefab>();
        builder.AppBuilder.ViewLocator.RegisterViewFor<
            MapPreloadDialogViewModel,
            MapPreloadDialogView
        >();

        builder.AppBuilder.Dialogs.RegisterPrefab<GeoPointDialogPrefab>();
        builder.AppBuilder.ViewLocator.RegisterViewFor<
            GeoPointDialogViewModel,
            GeoPointDialogView
        >();

        builder.AppBuilder.Dialogs.RegisterPrefab<EditApiKeyDialogPrefab>();
        builder.AppBuilder.ViewLocator.RegisterViewFor<
            EditApiKeyDialogViewModel,
            EditApiKeyDialogView
        >();

        return builder;
    }
}

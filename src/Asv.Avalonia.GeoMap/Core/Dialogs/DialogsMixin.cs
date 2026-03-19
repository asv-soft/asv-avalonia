namespace Asv.Avalonia.GeoMap;

public static class DialogsMixin
{
    public static GeoMapMixin.Builder AddDialogs(this GeoMapMixin.Builder builder)
    {
        builder.Parent.Dialogs.RegisterPrefab<GeoPointDialogPrefab>();
        builder.Parent.ViewLocator.RegisterViewFor<GeoPointDialogViewModel, GeoPointDialogView>();

        return builder;
    }
}

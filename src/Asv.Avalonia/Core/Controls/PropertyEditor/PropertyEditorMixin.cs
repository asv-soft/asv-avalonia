namespace Asv.Avalonia;

public static class PropertyEditorMixin
{
    extension(ViewLocatorMixin.Builder builder)
    {
        public ViewLocatorMixin.Builder RegisterPropertyEditor()
        {
            return builder
                .RegisterViewFor<PropertyEditorViewModel, PropertyEditorView>()
                .RegisterViewFor<UnitPropertyViewModel, UnitPropertyView>()
                .RegisterViewFor<GeoPointPropertyViewModel, GeoPointPropertyView>();
        }
    }
}

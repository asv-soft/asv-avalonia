namespace Asv.Avalonia;

public static class PropertyEditorMixin
{
    extension(ViewLocatorMixin.Builder builder)
    {
        public ViewLocatorMixin.Builder RegisterPropertyEditor()
        {
            return builder
                .RegisterViewFor<PropertyEditorViewModel, PropertyEditorView>()
                .RegisterViewFor<PropertyTextBoxViewModel, PropertyTextBoxView>()
                .RegisterViewFor<PropertyComboBoxViewModel, PropertyComboBoxView>()
                .RegisterViewFor<PropertyUnitViewModel, PropertyUnitView>()
                .RegisterViewFor<PropertyGeoPointViewModel, PropertyGeoPointView>()
                .RegisterViewFor<PropertyButtonViewModel, PropertyButtonView>();
        }
    }
}

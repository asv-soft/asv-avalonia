namespace Asv.Avalonia;

public static class PropertyEditorMixin
{
    extension(ViewLocatorMixin.Builder builder)
    {
        public ViewLocatorMixin.Builder RegisterPropertyEditor()
        {
            return builder
                .RegisterViewFor<PropertyEditorViewModel, PropertyEditorView>()
                .RegisterViewFor<ExtendedPropertyEditorViewModel, ExtendedPropertyEditorView>()
                .RegisterViewFor<PropertyTextBoxViewModel, PropertyTextBoxView>()
                .RegisterViewFor<PropertyComboBoxViewModel, PropertyComboBoxView>()
                .RegisterViewFor<
                    PropertyToggleButtonGroupViewModel,
                    PropertyToggleButtonGroupView
                >()
                .RegisterViewFor<PropertyUnitViewModel, PropertyUnitView>()
                .RegisterViewFor<PropertySliderViewModel, PropertySliderView>()
                .RegisterViewFor<PropertyToggleSwitchViewModel, PropertyToggleSwitchView>()
                .RegisterViewFor<PropertyButtonViewModel, PropertyButtonView>();
        }
    }
}

namespace Asv.Avalonia;

public static class PropertyEditorRegistrations
{
    extension(ControlsRegistrations.Builder builder)
    {
        public ControlsRegistrations.Builder RegisterPropertyEditor()
        {
            builder
                .AppBuilder.ViewLocator.RegisterViewFor<
                    PropertyEditorViewModel,
                    PropertyEditorView
                >()
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

            return builder;
        }
    }
}

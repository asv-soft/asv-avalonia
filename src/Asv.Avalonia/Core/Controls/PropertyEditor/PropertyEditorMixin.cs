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
                .RegisterViewFor<PropertyUnitViewModel, PropertyUnitView>()
                .RegisterViewFor<PropertyButtonViewModel, PropertyButtonView>();
        }
    }
}

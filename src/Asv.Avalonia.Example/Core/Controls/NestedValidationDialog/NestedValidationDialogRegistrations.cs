using Asv.Avalonia;

namespace Asv.Avalonia.Example;

public static class NestedValidationDialogRegistrations
{
    extension(ExampleControlsRegistrations.Builder builder)
    {
        public ExampleControlsRegistrations.Builder RegisterNestedValidationDialog()
        {
            builder.AppBuilder.ViewLocator.RegisterViewFor<
                NestedValidationDialogViewModel,
                NestedValidationDialogView
            >();
            return builder;
        }
    }
}

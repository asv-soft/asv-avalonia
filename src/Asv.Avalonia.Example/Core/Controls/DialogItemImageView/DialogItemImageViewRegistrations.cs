using Asv.Avalonia;

namespace Asv.Avalonia.Example;

public static class DialogItemImageViewRegistrations
{
    extension(ExampleControlsRegistrations.Builder builder)
    {
        public ExampleControlsRegistrations.Builder RegisterDialogItemImageView()
        {
            builder.AppBuilder.ViewLocator.RegisterViewFor<
                DialogItemImageViewModel,
                DialogItemImageView
            >();
            return builder;
        }
    }
}

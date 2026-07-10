namespace Asv.Avalonia;

public static class SearchBoxRegistrations
{
    extension(ControlsRegistrations.Builder builder)
    {
        public ControlsRegistrations.Builder RegisterSearchBox()
        {
            builder.AppBuilder.ViewLocator.RegisterViewFor<SearchBoxViewModel, SearchBoxView>();
            return builder;
        }
    }
}

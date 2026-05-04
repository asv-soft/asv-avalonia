namespace Asv.Avalonia;

public static class SearchBoxMixin
{
    public static ViewLocatorMixin.Builder RegisterSearchBox(this ViewLocatorMixin.Builder builder)
    {
        return builder.RegisterViewFor<SearchBoxViewModel, SearchBoxView>();
    }
}

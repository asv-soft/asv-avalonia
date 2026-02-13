namespace Asv.Avalonia;

public static class AppHostSearchBox
{
    public static AppHostControls.Builder RegisterSearchBox(this AppHostControls.Builder builder)
    {
        return builder.RegisterViewFor<SearchBoxViewModel, SearchBoxView>();
    }
}

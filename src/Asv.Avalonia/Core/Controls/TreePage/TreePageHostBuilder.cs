namespace Asv.Avalonia;

public static class TreePageHostBuilder
{
    public static ViewLocatorMixin.Builder RegisterTreePage(this ViewLocatorMixin.Builder builder)
    {
        return builder.RegisterViewFor<GroupTreePageItemViewModel, GroupTreePageItemView>();
    }
}

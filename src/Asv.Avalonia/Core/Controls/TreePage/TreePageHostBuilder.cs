namespace Asv.Avalonia;

public static class TreePageHostBuilder
{
    public static AppHostControls.Builder RegisterTreePage(this AppHostControls.Builder builder)
    {
        return builder.RegisterViewFor<GroupTreePageItemViewModel, GroupTreePageItemView>();
    }
}

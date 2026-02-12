namespace Asv.Avalonia;

public static class TreePageHostBuilder
{
    public static ControlsHostBuilder.Builder RegisterTreePage(this ControlsHostBuilder.Builder builder)
    {
        return builder
            .RegisterViewFor<GroupTreePageItemViewModel, GroupTreePageItemView>();
    }
}
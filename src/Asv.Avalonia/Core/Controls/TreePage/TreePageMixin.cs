namespace Asv.Avalonia;

public static class TreePageMixin
{
    public static ControlsHostBuilder RegisterTreePage(this ControlsHostBuilder builder)
    {
        return builder
            .Register<GroupTreePageItemViewModel, GroupTreePageItemView>();
    }
}
namespace Asv.Avalonia;

public static class WorkspaceMixin
{
    public static ControlsHostBuilder RegisterWorkspace(this ControlsHostBuilder builder)
    {
        builder.Register<StackPanelWidgetViewModel, StackPanelWidgetView>();
        return builder;
    }
}

namespace Asv.Avalonia;

public static class WorkspaceMixin
{
    public static ControlsHostBuilder RegisterWorkspace(this ControlsHostBuilder builder)
    {
        return builder
            .Register<StackPanelWidgetViewModel, StackPanelWidgetView>()
            .Register<WrapPanelWidgetViewModel, WrapPanelWidgetView>();
    }
}

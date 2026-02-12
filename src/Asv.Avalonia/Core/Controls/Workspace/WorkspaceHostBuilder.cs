namespace Asv.Avalonia;

public static class WorkspaceHostBuilder
{
    public static ControlsHostBuilder.Builder RegisterWorkspace(this ControlsHostBuilder.Builder builder)
    {
        return builder
            .RegisterViewFor<StackPanelWidgetViewModel, StackPanelWidgetView>()
            .RegisterViewFor<WrapPanelWidgetViewModel, WrapPanelWidgetView>();
    }
}

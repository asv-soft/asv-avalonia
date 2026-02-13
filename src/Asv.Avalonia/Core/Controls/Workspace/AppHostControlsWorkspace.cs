namespace Asv.Avalonia;

public static class AppHostControlsWorkspace
{
    public static AppHostControls.Builder RegisterWorkspace(this AppHostControls.Builder builder)
    {
        return builder
            .RegisterViewFor<StackPanelWidgetViewModel, StackPanelWidgetView>()
            .RegisterViewFor<WrapPanelWidgetViewModel, WrapPanelWidgetView>();
    }
}

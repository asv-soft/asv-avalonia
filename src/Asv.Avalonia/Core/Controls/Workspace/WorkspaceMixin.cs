namespace Asv.Avalonia;

public static class WorkspaceMixin
{
    public static ViewLocatorMixin.Builder RegisterWorkspace(this ViewLocatorMixin.Builder builder)
    {
        return builder
            .RegisterViewFor<StackPanelWidgetViewModel, StackPanelWidgetView>()
            .RegisterViewFor<WrapPanelWidgetViewModel, WrapPanelWidgetView>();
    }
}

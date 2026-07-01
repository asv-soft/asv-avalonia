namespace Asv.Avalonia;

public static class WorkspaceRegistrations
{
    extension(ControlsRegistrations.Builder builder)
    {
        public ControlsRegistrations.Builder RegisterWorkspace()
        {
            builder
                .AppBuilder.ViewLocator.RegisterViewFor<
                    StackPanelWidgetViewModel,
                    StackPanelWidgetView
                >()
                .RegisterViewFor<WrapPanelWidgetViewModel, WrapPanelWidgetView>();

            return builder;
        }
    }
}

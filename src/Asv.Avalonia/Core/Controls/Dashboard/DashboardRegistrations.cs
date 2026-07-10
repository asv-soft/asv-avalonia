namespace Asv.Avalonia;

public static class DashboardRegistrations
{
    extension(ControlsRegistrations.Builder builder)
    {
        public ControlsRegistrations.Builder RegisterDashboard()
        {
            builder
                .AppBuilder.ViewLocator.RegisterViewFor<IDashboard, DashboardView>()
                .RegisterViewFor<DashboardViewModel, DashboardView>()
                .RegisterViewFor<TextTileViewModel, TextTileView>();

            return builder;
        }
    }
}

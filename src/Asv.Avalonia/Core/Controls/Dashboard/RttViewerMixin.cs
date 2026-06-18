namespace Asv.Avalonia;

public static class DashboardMixin
{
    extension(ViewLocatorMixin.Builder builder)
    {
        public ViewLocatorMixin.Builder RegisterDashboard()
        {
            return builder
                .RegisterViewFor<IDashboard, DashboardView>()
                .RegisterViewFor<DashboardViewModel, DashboardView>()
                .RegisterViewFor<TextTileViewModel, TextTileView>();
        }
    }
}

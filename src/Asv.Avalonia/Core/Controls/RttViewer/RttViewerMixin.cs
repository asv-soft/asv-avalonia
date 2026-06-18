namespace Asv.Avalonia;

public static class RttViewerMixin
{
    extension(ViewLocatorMixin.Builder builder)
    {
        public ViewLocatorMixin.Builder RegisterRttViewer()
        {
            return builder
                .RegisterViewFor<ITileDashboardViewModel, TileDashboardView>()
                .RegisterViewFor<TileDashboardViewModel, TileDashboardView>()
                .RegisterViewFor<TextTileViewModel, TextTileView>();
        }
    }
}

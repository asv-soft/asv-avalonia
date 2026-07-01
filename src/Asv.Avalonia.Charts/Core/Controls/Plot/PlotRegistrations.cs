using Asv.Avalonia;

namespace Asv.Avalonia.Charts;

public static class PlotRegistrations
{
    extension(ControlsRegistrations.Builder builder)
    {
        public ControlsRegistrations.Builder RegisterPlot()
        {
            builder.AppBuilder.ViewLocator.RegisterViewFor<PlotViewModel, PlotView>();
            return builder;
        }
    }
}

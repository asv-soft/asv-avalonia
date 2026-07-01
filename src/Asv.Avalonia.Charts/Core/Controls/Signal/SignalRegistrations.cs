using Asv.Avalonia;

namespace Asv.Avalonia.Charts;

public static class SignalRegistrations
{
    extension(ControlsRegistrations.Builder builder)
    {
        public ControlsRegistrations.Builder RegisterSignal()
        {
            builder.AppBuilder.ViewLocator.RegisterViewFor<ISignalPlotWidget, PlotView>();
            return builder;
        }
    }
}

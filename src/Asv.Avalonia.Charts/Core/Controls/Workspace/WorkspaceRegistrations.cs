using Asv.Avalonia;

namespace Asv.Avalonia.Charts;

public static class WorkspaceRegistrations
{
    extension(ControlsRegistrations.Builder builder)
    {
        public ControlsRegistrations.Builder RegisterWorkspace()
        {
            builder.AppBuilder.ViewLocator.RegisterViewFor<ISignalPlotWidget, PlotView>();
            return builder;
        }
    }
}

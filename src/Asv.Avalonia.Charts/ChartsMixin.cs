using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Charts;

public static class ChartsMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseModuleCharts(Action<Builder>? configure = null)
        {
            configure ??= b =>
            {
                b.RegisterDefault();
            };
            configure(new Builder(builder));
            return builder;
        }

        public Builder ModuleCharts => new(builder);
    }

    public class Builder(IHostApplicationBuilder builder)
    {
        public void RegisterDefault()
        {
            builder
                .ViewLocator.RegisterViewFor<PlotViewModel, PlotView>()
                .RegisterViewFor<IPlotWidget, PlotView>()
                .RegisterViewFor<ISignalPlotWidget, PlotView>();
        }

        public IHostApplicationBuilder Parent => builder;
    }
}

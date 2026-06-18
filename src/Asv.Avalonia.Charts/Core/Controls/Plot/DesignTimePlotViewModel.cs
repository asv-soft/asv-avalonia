using Asv.Common;
using R3;
using ScottPlot.Plottables;

namespace Asv.Avalonia.Charts;

public class DesignTimePlotViewModel : PlotViewModel
{
    private double? _lastValue;

    public DesignTimePlotViewModel()
        : base(DesignTime.Id.TypeId, DesignTime.ThemeService)
    {
        DesignTime.ThrowIfNotDesignMode();
        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                _lastValue = new Random().NextDouble() * 100;
                Refresh();
            })
            .DisposeItWith(Disposable);
    }

    protected override void BeginDraw() { }

    protected override void Draw(AvaPlot panel)
    {
        if (panel.Plot.PlottableList.FirstOrDefault() is not DataStreamer distanceChart)
        {
            var plot = panel.Plot;
            distanceChart = plot.Add.DataStreamer(100);
            distanceChart.ViewScrollRight();
            plot.Axes.Left.Label.Text = "Plot (m)";
            plot.Axes.Bottom.Label.Text = "Time (s)";
        }

        if (_lastValue != null)
        {
            distanceChart.Add(_lastValue.Value);
        }
    }

    protected override void EndDraw()
    {
        _lastValue = null;
    }
}

using Avalonia.Controls;
using Avalonia.Interactivity;
using R3;

namespace Asv.Avalonia.Charts;

public partial class PlotView : UserControl
{
    private const string AvaPlotName = "AvaPlot";
    private PlotViewModel? _lastDataContext;

    public PlotView()
    {
        InitializeComponent();

        this.ObservePropertyChanged(x => x.DataContext)
            .Select(x => x as PlotViewModel)
            .WhereNotNull()
            .Subscribe(x =>
            {
                var avaPlot = this.FindControl<AvaPlot>(AvaPlotName);
                if (avaPlot == null)
                {
                    return;
                }
                x.AddView(avaPlot);
                _lastDataContext = x;
            });
        this.ObservePropertyChanged(x => x.DataContext)
            .Select(x => x is not PlotViewModel)
            .Subscribe(x =>
            {
                var avaPlot = this.FindControl<AvaPlot>(AvaPlotName);
                if (avaPlot == null)
                {
                    return;
                }

                _lastDataContext?.RemoveView(avaPlot);
            });
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        var avaPlot = this.FindControl<AvaPlot>(AvaPlotName);
        if (DataContext is PlotViewModel vm && avaPlot != null)
        {
            vm.AddView(avaPlot);
        }
        base.OnLoaded(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        var avaPlot = this.FindControl<AvaPlot>(AvaPlotName);
        if (DataContext is PlotViewModel vm && avaPlot != null)
        {
            vm.RemoveView(avaPlot);
        }
    }
}

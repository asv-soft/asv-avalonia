using Asv.Common;
using Avalonia.Media;
using R3;
using ScottPlot;
using ScottPlot.Plottables;
using Color = ScottPlot.Color;

namespace Asv.Avalonia.Charts;

public class SignalPlotViewModel : PlotViewModel, ISignalPlot
{
    private double[]? _data;
    private readonly Color _transparrent = Color.FromARGB(0U);
    private Color _lineColor = Color.FromHex("#A38F2D");
    private bool _isLayoutLoaded;

    public SignalPlotViewModel()
        : this(DesignTime.Id.TypeId, DesignTime.ThemeService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public SignalPlotViewModel(string typeId, IThemeService themeService)
        : base(typeId, themeService)
    {
        RegisterLayout();
    }

    public IBrush? LineColor
    {
        get;
        set
        {
            if (SetField(ref field, value))
            {
                if (field is SolidColorBrush brush)
                {
                    _lineColor = new Color(
                        brush.Color.R,
                        brush.Color.G,
                        brush.Color.B,
                        brush.Color.A
                    );
                    Refresh();
                }
            }
        }
    }

    public int HistorySize
    {
        get;
        set
        {
            if (SetField(ref field, value))
            {
                Refresh();
            }
        }
    } = 5;

    protected override void BeginDraw() { }

    protected override void Draw(AvaPlot panel)
    {
        while (panel.Plot.PlottableList.Count > HistorySize)
        {
            panel.Plot.PlottableList.RemoveAt(0);
        }

        if (_data != null)
        {
            panel.Plot.Add.Signal(_data);
        }

        for (var i = 0; i < panel.Plot.PlottableList.Count; i++)
        {
            var signal = (Signal)panel.Plot.PlottableList[i];
            signal.LineColor = _lineColor.WithAlpha((double)i / panel.Plot.PlottableList.Count);
            signal.LineWidth = i == panel.Plot.PlottableList.Count - 1 ? 2 : 1;
        }

        panel.Plot.Axes.AutoScaleX();
        panel.Plot.Axes.AutoScaleY();
        panel.Plot.Grid.LineColor = _lineColor.WithAlpha(0.3);
        panel.Plot.Grid.LinePattern = LinePattern.Dashed;
    }

    protected override void RefreshStyles(Plot plot)
    {
        base.RefreshStyles(plot);
        plot.DataBackground.Color = _transparrent;
        plot.FigureBackground.Color = _transparrent;
        plot.Axes.Frameless();
    }

    protected override void EndDraw()
    {
        _data = null;
    }

    public void Refresh(double[] data)
    {
        _data = data;
        Refresh();
    }

    private void RegisterLayout()
    {
        var historySizeLayout = Layout.Register<int>(
            nameof(HistorySize),
            (value, _) =>
            {
                if (value > 0)
                {
                    HistorySize = value;
                }

                return ValueTask.CompletedTask;
            }
        );
        var historySizeLayoutSave = this.ObservePropertyChanged(x => x.HistorySize)
            .Where(_ => _isLayoutLoaded)
            .SubscribeAwait(
                (_, cancel) => historySizeLayout.SaveAsync(HistorySize, cancel),
                AwaitOperation.Drop
            );
        R3.Disposable.Combine(historySizeLayout, historySizeLayoutSave).DisposeItWith(Disposable);

        RootTracking.ExecuteWhenRootAttached(LoadLayoutWhenRootAttached).DisposeItWith(Disposable);
        return;

        async ValueTask LoadLayoutWhenRootAttached(IShell root, CancellationToken cancel)
        {
            _ = root;
            _isLayoutLoaded = false;
            try
            {
                await historySizeLayout.LoadAsync(cancel);
            }
            finally
            {
                if (!cancel.IsCancellationRequested && !IsDisposed)
                {
                    _isLayoutLoaded = true;
                }
            }
        }
    }
}

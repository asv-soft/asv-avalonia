using Asv.Common;
using R3;
using ScottPlot;
using ScottPlot.Palettes;

namespace Asv.Avalonia.Charts;

public abstract class PlotViewModel : ViewModel
{
    private readonly IThemeService _themeService;
    private readonly HashSet<AvaPlot> _panels = new(1);
    private readonly Subject<Unit> _requestRefresh;

    #region Desgin time

    public PlotViewModel()
        : this(DesignTime.Id.TypeId, DesignTime.ThemeService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    #endregion

    public PlotViewModel(string typeId, IThemeService themeService)
        : base(typeId)
    {
        _themeService = themeService;
        _requestRefresh = new Subject<Unit>().DisposeItWith(Disposable);
        _requestRefresh
            .ThrottleLastFrame(1)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(Refresh)
            .DisposeItWith(Disposable);
        Disposable.AddAction(() =>
        {
            var itemsToDelete = _panels.ToArray();
            foreach (var avaPlot in itemsToDelete)
            {
                RemoveView(avaPlot);
            }
        });
        themeService
            .CurrentTheme.DistinctUntilChanged()
            .Subscribe(_ => RefreshStyles())
            .DisposeItWith(Disposable);

        Events.Catch<RefreshPlotEvent>(OnRefreshPlotEvent).DisposeItWith(Disposable);
    }

    private void OnRefreshPlotEvent(RefreshPlotEvent e)
    {
        Refresh();
        e.IsHandled = true;
    }

    #region Refresh

    private void Refresh(Unit unit)
    {
        if (IsDisposed)
        {
            return;
        }
        BeginDraw();
        foreach (var panel in _panels)
        {
            Draw(panel);
            panel.Refresh();
        }
        EndDraw();
    }

    protected abstract void BeginDraw();
    protected abstract void Draw(AvaPlot panel);
    protected abstract void EndDraw();

    #endregion

    #region Add\Remove views

    internal void AddView(AvaPlot panel)
    {
        if (_panels.Add(panel))
        {
            panel.Multiplot.Subplots.PlotAddedAction += OnSubplotsPlotAddedAction;
        }

        RefreshStyles();
        Refresh();
    }

    internal void RemoveView(AvaPlot avaPlot)
    {
        if (_panels.Remove(avaPlot))
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            avaPlot.Multiplot.Subplots.PlotAddedAction -= OnSubplotsPlotAddedAction;
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        avaPlot.Multiplot.Reset();
    }

    #endregion

    private void OnSubplotsPlotAddedAction(List<Plot> plot)
    {
        foreach (var item in plot)
        {
            RefreshStyles(item);
        }
    }

    public void Refresh()
    {
        if (_requestRefresh.IsDisposed)
        {
            return;
        }

        _requestRefresh.OnNext(Unit.Default);
    }

    public void AutoScale()
    {
        foreach (var panel in _panels)
        {
            for (var i = 0; i < panel.Multiplot.Subplots.Count; i++)
            {
                panel.Multiplot.Subplots.GetPlot(i).Axes.AutoScale();
            }

            panel.Refresh();
        }
    }

    #region Styles

    private void RefreshStyles()
    {
        foreach (var panel in _panels)
        {
            RefreshStyles(panel);
        }
    }

    private void RefreshStyles(AvaPlot panel)
    {
        for (var i = 0; i < panel.Multiplot.Subplots.Count; i++)
        {
            RefreshStyles(panel.Multiplot.Subplots.GetPlot(i));
        }
    }

    protected virtual void RefreshStyles(Plot plot)
    {
        if (_themeService.CurrentTheme.CurrentValue?.Id == ThemeService.LightTheme)
        {
            // ���������� ����������� ������� �������
            plot.Add.Palette = new Category10();

            // ��� ������ � ������� ������
            plot.FigureBackground.Color = Color.FromHex("#ffffff"); // �����
            plot.DataBackground.Color = Color.FromHex("#ffffff");

            // ��� � �����
            plot.Axes.Color(Color.FromHex("#000000")); // ������ ��� � �������
            plot.Grid.MajorLineColor = Color.FromHex("#e0e0e0"); // ������-����� �����

            // �������
            plot.Legend.BackgroundColor = Color.FromHex("#f0f0f0"); // ������-����� ���
            plot.Legend.FontColor = Color.FromHex("#000000"); // ������ �����
            plot.Legend.OutlineColor = Color.FromHex("#000000"); // ������ �����
        }
        else
        {
            plot.Add.Palette = new Penumbra();

            // change figure colors
            plot.FigureBackground.Color = Color.FromHex("#181818");
            plot.DataBackground.Color = Color.FromHex("#1f1f1f");

            // change axis and grid colors
            plot.Axes.Color(Color.FromHex("#d7d7d7"));
            plot.Grid.MajorLineColor = Color.FromHex("#404040");

            // change legend colors
            plot.Legend.BackgroundColor = Color.FromHex("#404040");
            plot.Legend.FontColor = Color.FromHex("#d7d7d7");
            plot.Legend.OutlineColor = Color.FromHex("#d7d7d7");
        }
    }

    #endregion

    public override IEnumerable<IViewModel> GetChildren()
    {
        return [];
    }
}

using Asv.Common;
using Asv.Modeling;
using Avalonia.Controls;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia.Charts;

public class SignalPlotWidget : SignalPlotViewModel, ISignalPlotWidget
{
    private const string HistorySizeMenuId = "history-size";
    private const string HistorySizeMenuGroupName = "signal-plot-history-size";
    private static readonly int[] HistorySizeItems = [1, 3, 5, 10];
    private readonly List<MenuItem> _historySizeMenuItems = [];

    public SignalPlotWidget()
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public SignalPlotWidget(string typeId, IThemeService themeService)
        : base(typeId, themeService)
    {
        Menu.SetParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
        AddHistorySizeMenu();
        this.ObservePropertyChanged(x => x.HistorySize)
            .Subscribe(_ => UpdateHistorySizeMenu())
            .DisposeItWith(Disposable);
    }

    public ObservableList<IMenuItem> Menu { get; } = [];

    public MaterialIconKind? Icon
    {
        get;
        set => SetField(ref field, value);
    }

    public AsvColorKind IconColor
    {
        get;
        set => SetField(ref field, value);
    }

    public string? Header
    {
        get;
        set => SetField(ref field, value);
    }

    public WorkspaceDock Position
    {
        get;
        set => SetField(ref field, value);
    }

    public bool IsExpanded
    {
        get;
        set => SetField(ref field, value);
    }

    public bool CanExpand
    {
        get;
        set => SetField(ref field, value);
    }

    public MenuTree? MenuView { get; set; }

    public bool IsVisible
    {
        get;
        set => SetField(ref field, value);
    } = true;

    public int Order { get; set; }

    private void AddHistorySizeMenu()
    {
        var historySizeMenu = new MenuItem(HistorySizeMenuId, "History size")
        {
            Icon = MaterialIconKind.History,
        };
        Menu.Add(historySizeMenu);

        foreach (var historySize in HistorySizeItems)
        {
            var item = new MenuItem(
                $"{HistorySizeMenuId}-{historySize}",
                historySize.ToString(),
                historySizeMenu.Id.TypeId
            )
            {
                ToggleType = MenuItemToggleType.Radio,
                GroupName = HistorySizeMenuGroupName,
                Command = new ReactiveCommand(_ => HistorySize = historySize),
            };
            _historySizeMenuItems.Add(item);
            Menu.Add(item);
        }

        UpdateHistorySizeMenu();
    }

    private void UpdateHistorySizeMenu()
    {
        foreach (var item in _historySizeMenuItems)
        {
            item.IsChecked = item.Id.TypeId == $"{HistorySizeMenuId}-{HistorySize}";
        }
    }
}

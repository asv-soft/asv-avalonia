using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using ObservableCollections;

namespace Asv.Avalonia.Charts;

public abstract class PlotWidget : PlotViewModel, IPlotWidget
{
    public PlotWidget(string typeId, IThemeService themeService)
        : base(typeId, themeService)
    {
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
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
}

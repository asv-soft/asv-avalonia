using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using ObservableCollections;

namespace Asv.Avalonia.GeoMap;

public class MapWidget : MapViewModel, IWorkspaceWidget
{
    public MapWidget(string id, IMapService mapService, IExtensionService extension)
        : base(id, mapService, extension)
    {
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
    }

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

    public ObservableList<IMenuItem> Menu { get; } = [];

    public MenuTree? MenuView { get; }

    public bool IsVisible
    {
        get;
        set => SetField(ref field, value);
    } = true;

    public int Order { get; set; }
}

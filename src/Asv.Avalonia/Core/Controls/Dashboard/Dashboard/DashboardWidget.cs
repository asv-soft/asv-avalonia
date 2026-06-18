using System.ComponentModel;
using Asv.Common;
using Asv.Modeling;
using Avalonia.Controls;
using Material.Icons;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class DashboardWidget : DashboardViewModel, IWorkspaceWidget
{
    private const string TileMenuIdPrefix = "tile";
    private static readonly TileDensity[] DensityItems =
    [
        TileDensity.Regular,
        TileDensity.Compact,
        TileDensity.Inline,
    ];
    private readonly Dictionary<ITileViewModel, TileMenuState> _tileMenus = [];

    public DashboardWidget()
        : this(NavId.GenerateRandomAsString())
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public DashboardWidget(string typeId)
        : base(typeId)
    {
        Menu.SetRoutableParent(this).DisposeItWith(Disposable);
        Menu.DisposeRemovedItems().DisposeItWith(Disposable);
        MenuView = new MenuTree(Menu).DisposeItWith(Disposable);
        Tiles.ObserveAdd().Subscribe(x => AddTileMenu(x.Value)).DisposeItWith(Disposable);
        Tiles.ObserveRemove().Subscribe(x => RemoveTileMenu(x.Value)).DisposeItWith(Disposable);
        Disposable.AddAction(RemoveTileMenus);

        foreach (var tile in Tiles)
        {
            AddTileMenu(tile);
        }
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
    }

    public int Order { get; set; }

    private void AddTileMenu(ITileViewModel tile)
    {
        if (_tileMenus.ContainsKey(tile))
        {
            return;
        }

        var tileMenu = new MenuItem(GetTileMenuId(tile), GetTileHeader(tile))
        {
            Icon = tile.Icon,
            IconColor = tile.IconColor,
            Order = tile.Order,
        };
        var state = new TileMenuState(tileMenu);
        PropertyChangedEventHandler handler = (_, e) => UpdateTileMenu(tile, e.PropertyName);
        tile.PropertyChanged += handler;
        state.Disposable.Add(R3.Disposable.Create(() => tile.PropertyChanged -= handler));

        Menu.Add(tileMenu);

        foreach (var density in DensityItems)
        {
            var item = new MenuItem(
                GetTileDensityMenuId(tile, density),
                density.ToString(),
                tileMenu.Id.TypeId
            )
            {
                ToggleType = MenuItemToggleType.Radio,
                GroupName = GetTileDensityGroupName(tile),
                Order = (int)density,
                Command = new ReactiveCommand(_ => tile.Density = density).DisposeItWith(
                    state.Disposable
                ),
            };
            state.DensityItems[density] = item;
            Menu.Add(item);
        }

        state.VisibilityItem = new MenuItem(
            GetTileVisibilityMenuId(tile),
            "Show/Hide",
            tileMenu.Id.TypeId
        )
        {
            ToggleType = MenuItemToggleType.CheckBox,
            Order = DensityItems.Length,
            Command = new ReactiveCommand(_ => tile.IsVisible = !tile.IsVisible).DisposeItWith(
                state.Disposable
            ),
        };
        Menu.Add(state.VisibilityItem);

        _tileMenus[tile] = state;
        UpdateTileMenu(tile, null);
    }

    private void RemoveTileMenu(ITileViewModel tile)
    {
        if (!_tileMenus.Remove(tile, out var state))
        {
            return;
        }

        state.Disposable.Dispose();

        foreach (var item in state.DensityItems.Values)
        {
            Menu.Remove(item);
        }

        if (state.VisibilityItem is not null)
        {
            Menu.Remove(state.VisibilityItem);
        }

        Menu.Remove(state.TileItem);
    }

    private void RemoveTileMenus()
    {
        foreach (var tile in _tileMenus.Keys.ToArray())
        {
            RemoveTileMenu(tile);
        }
    }

    private void UpdateTileMenu(ITileViewModel tile, string? propertyName)
    {
        if (!_tileMenus.TryGetValue(tile, out var state))
        {
            return;
        }

        if (
            propertyName is null
            || propertyName == nameof(ITileViewModel.Header)
            || propertyName == nameof(ITileViewModel.Icon)
            || propertyName == nameof(ITileViewModel.IconColor)
        )
        {
            state.TileItem.Header = GetTileHeader(tile);
            state.TileItem.Icon = tile.Icon;
            state.TileItem.IconColor = tile.IconColor;
        }

        if (propertyName is null || propertyName == nameof(ITileViewModel.Density))
        {
            foreach (var item in state.DensityItems)
            {
                item.Value.IsChecked = item.Key == tile.Density;
            }
        }

        if (
            propertyName is null
            || propertyName == nameof(ITileViewModel.IsVisible)
            || propertyName == nameof(IHeadlinedViewModel.IsVisible)
        )
        {
            if (state.VisibilityItem is not null)
            {
                state.VisibilityItem.IsChecked = tile.IsVisible;
            }
        }
    }

    private static string GetTileHeader(ITileViewModel tile)
    {
        return string.IsNullOrWhiteSpace(tile.Header) ? tile.Id.TypeId : tile.Header;
    }

    private static string GetTileMenuId(ITileViewModel tile)
    {
        return $"{TileMenuIdPrefix}-{tile.Id.TypeId}";
    }

    private static string GetTileDensityGroupName(ITileViewModel tile)
    {
        return $"{GetTileMenuId(tile)}-density";
    }

    private static string GetTileDensityMenuId(ITileViewModel tile, TileDensity density)
    {
        return $"{GetTileMenuId(tile)}-density-{density}";
    }

    private static string GetTileVisibilityMenuId(ITileViewModel tile)
    {
        return $"{GetTileMenuId(tile)}-visibility";
    }

    private sealed class TileMenuState(MenuItem tileItem) : IDisposable
    {
        public MenuItem TileItem { get; } = tileItem;

        public Dictionary<TileDensity, MenuItem> DensityItems { get; } = [];

        public MenuItem? VisibilityItem { get; set; }

        public CompositeDisposable Disposable { get; } = [];

        public void Dispose()
        {
            Disposable.Dispose();
        }
    }
}

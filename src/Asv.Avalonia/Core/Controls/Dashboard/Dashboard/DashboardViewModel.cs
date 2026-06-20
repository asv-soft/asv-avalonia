using Asv.Common;
using Asv.Modeling;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class DashboardViewModel : ViewModel, IDashboard
{
    private readonly Dictionary<ITileViewModel, IDisposable> _densitySubscriptions = [];
    private readonly ObservableList<ITileViewModel> _regularTiles = [];
    private readonly ObservableList<ITileViewModel> _inlineTiles = [];

    public DashboardViewModel()
        : this(NavId.GenerateRandomAsString())
    {
        DesignTime.ThrowIfNotDesignMode();
        Tiles.Add(
            new TextTileViewModel("gnss-1")
            {
                Density = TileDensity.Regular,
                Header = "GNSS 1 Status",
                ShortHeader = "GNSS1",
                Text = "123",
                SubScriptText = ".456",
                Units = "deg",
                StatusText = "15.6 V / 4.8 A / 74.9 W",
                Progress = 0.7,
            }
        );
        Tiles.Add(
            new TextTileViewModel("gnss-2")
            {
                Density = TileDensity.Regular,
                Header = "GNSS 2 Status",
                ShortHeader = "GNSS2",
                Text = "RTK Fixed",
                Progress = double.NegativeInfinity,
            }
        );
        Tiles.Add(
            new TextTileViewModel("gnss-3")
            {
                Density = TileDensity.Regular,
                Header = "GNSS 3 Status",
                ShortHeader = "GNSS3",
                Text = "RTK Float",
            }
        );
        Tiles.Add(
            new TextTileViewModel("gps")
            {
                Density = TileDensity.Inline,
                Header = "GPS Status",
                ShortHeader = "GNSS",
                Text = "RTK Fixed",
            }
        );
    }

    public DashboardViewModel(string typeId, NavArgs args = default)
        : base(typeId, args)
    {
        Tiles.SetRoutableParent(this).DisposeItWith(Disposable);
        Tiles.DisposeRemovedItems().DisposeItWith(Disposable);
        Tiles.ObserveAdd().Subscribe(x => AddTile(x.Value)).DisposeItWith(Disposable);
        Tiles.ObserveRemove().Subscribe(x => RemoveTile(x.Value)).DisposeItWith(Disposable);

        Regular = _regularTiles.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
        Inline = _inlineTiles.ToNotifyCollectionChangedSlim().DisposeItWith(Disposable);
    }

    public ObservableList<ITileViewModel> Tiles { get; } = [];

    public NotifyCollectionChangedSynchronizedViewList<ITileViewModel> Regular { get; }

    public NotifyCollectionChangedSynchronizedViewList<ITileViewModel> Inline { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        return Tiles;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var subscription in _densitySubscriptions.Values)
            {
                subscription.Dispose();
            }

            _densitySubscriptions.Clear();
            _regularTiles.Clear();
            _inlineTiles.Clear();
            Tiles.ClearWithItemsDispose();
        }

        base.Dispose(disposing);
    }

    private void AddTile(ITileViewModel tile)
    {
        if (_densitySubscriptions.ContainsKey(tile))
        {
            MoveTile(tile);
            return;
        }

        tile.PropertyChanged += OnTilePropertyChanged;
        _densitySubscriptions[tile] = R3.Disposable.Create(() =>
            tile.PropertyChanged -= OnTilePropertyChanged
        );

        MoveTile(tile);
    }

    private void RemoveTile(ITileViewModel tile)
    {
        RemoveFromDensityLists(tile);

        if (_densitySubscriptions.Remove(tile, out var subscription))
        {
            subscription.Dispose();
        }
    }

    private void OnTilePropertyChanged(
        object? sender,
        System.ComponentModel.PropertyChangedEventArgs e
    )
    {
        if (e.PropertyName == nameof(ITileViewModel.Density) && sender is ITileViewModel tile)
        {
            MoveTile(tile);
        }
    }

    private void MoveTile(ITileViewModel tile)
    {
        RemoveFromDensityLists(tile);
        var target = GetDensityList(tile.Density);

        if (!target.Contains(tile))
        {
            InsertByOrder(target, tile);
        }
    }

    private void RemoveFromDensityLists(ITileViewModel tile)
    {
        _regularTiles.Remove(tile);
        _inlineTiles.Remove(tile);
    }

    private ObservableList<ITileViewModel> GetDensityList(TileDensity density)
    {
        return density switch
        {
            TileDensity.Inline => _inlineTiles,
            _ => _regularTiles,
        };
    }

    private static void InsertByOrder(ObservableList<ITileViewModel> target, ITileViewModel tile)
    {
        for (var i = 0; i < target.Count; i++)
        {
            if (tile.Order < target[i].Order)
            {
                target.Insert(i, tile);
                return;
            }
        }

        target.Add(tile);
    }
}

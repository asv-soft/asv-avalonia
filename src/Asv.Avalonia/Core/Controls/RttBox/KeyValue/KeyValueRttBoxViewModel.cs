using System.Diagnostics;
using Asv.Common;
using Asv.IO;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using ObservableCollections;
using R3;

namespace Asv.Avalonia;

public class KeyValueRttBoxViewModel : RttBoxViewModel
{
    private readonly ObservableList<KeyValueViewModel> _itemsSource;

    public KeyValueRttBoxViewModel()
        : this(DesignTime.Id.TypeId)
    {
        DesignTime.ThrowIfNotDesignMode();
        ShortHeader = "Short";
        ShortValueString = "0.00";
        ShortUnitSymbol = "ms";
        _itemsSource =
        [
            new(0) { Header = "Power", UnitSymbol = "dBm" },
            new(1) { Header = "Rise time", UnitSymbol = "ms" },
            new(2) { Header = "Fall time", UnitSymbol = "ms" },
            new(3) { Header = "Status", ValueString = "Normal" },
            new(4) { Header = "Unknown" },
        ];
        _itemsSource.DisposeRemovedItems().DisposeItWith(Disposable);
        Items = _itemsSource
            .ToNotifyCollectionChangedSlim(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);
        Icon = MaterialIconKind.Radar;
        Header = "Common RTT";

        int index = 0;
        int maxIndex = Enum.GetValues<AsvColorKind>().Length;
        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2))
            .Subscribe(_ =>
            {
                for (var i = 0; i < _itemsSource.Count; i++)
                {
                    var model = _itemsSource[i];
                    model.ValueString = (Random.Shared.NextDouble() * 1000.0).ToString($"F{i}");
                }

                Status = Enum.GetValues<AsvColorKind>()[index++ % maxIndex];
                ProgressStatus = Enum.GetValues<AsvColorKind>()[index++ % maxIndex];
                StatusText = Status.ToString();
                ShortValueString = (Random.Shared.NextDouble() * 1000.0).ToString("F2");
                Updated();
            })
            .DisposeItWith(Disposable);
    }

    public KeyValueRttBoxViewModel(
        string typeId,
        TimeSpan? networkErrorTimeout = null
    )
        : base(typeId, networkErrorTimeout)
    {
        _itemsSource = [];
        _itemsSource.DisposeRemovedItems().DisposeItWith(Disposable);
        Items = _itemsSource
            .ToNotifyCollectionChangedSlim(SynchronizationContextCollectionEventDispatcher.Current)
            .DisposeItWith(Disposable);
    }

    public NotifyCollectionChangedSynchronizedViewList<KeyValueViewModel> Items { get; }
    public ObservableList<KeyValueViewModel> ItemsSource => _itemsSource;

    public KeyValueViewModel this[int index, string header, string? unitSymbol]
    {
        get
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is out of range");
            }
            while (index >= _itemsSource.Count)
            {
                _itemsSource.Add(
                    new KeyValueViewModel(_itemsSource.Count)
                    {
                        Header = header,
                        UnitSymbol = unitSymbol,
                    }
                );
            }

            var item = _itemsSource[index];
            item.Header = header;
            item.UnitSymbol = unitSymbol;
            return item;
        }
    }

    public string? ShortValueString
    {
        get;
        set => SetField(ref field, value);
    }

    public string? ShortUnitSymbol
    {
        get;
        set => SetField(ref field, value);
    }

    public string? StatusText
    {
        get;
        set => SetField(ref field, value);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _itemsSource.RemoveAll();
        }

        base.Dispose(disposing);
    }
}

public class KeyValueRttBoxViewModel<T>
    : KeyValueRttBoxViewModel,
        IUpdatableRttBoxViewModel<KeyValueRttBoxViewModel<T>, T>
{
    private readonly TimeSpan? _networkErrorTimeout;

    public KeyValueRttBoxViewModel(
        string typeId,
        ILoggerFactory loggerFactory,
        Observable<T> valueStream,
        TimeSpan? networkErrorTimeout
    )
        : base(typeId, networkErrorTimeout)
    {
        _networkErrorTimeout = networkErrorTimeout;
        valueStream
            .ThrottleLastFrame(1)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(OnValueChanged)
            .DisposeItWith(Disposable);
    }

    public required Action<KeyValueRttBoxViewModel<T>, T> UpdateAction { get; init; }

    private void OnValueChanged(T value)
    {
        Debug.Assert(UpdateAction != null, "UpdateAction must be set");
        UpdateAction(this, value);
        if (_networkErrorTimeout != null)
        {
            Updated();
        }
    }
}

using System.Diagnostics;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class GeoPointRttBoxViewModel : RttBoxViewModel
{
    private readonly ReactiveProperty<GeoPoint> _location;
    private readonly IUnit _latitudeUnit;
    private readonly IUnit _longitudeUnit;
    private readonly IUnit _altitudeUnit;

    public GeoPointRttBoxViewModel()
        : this(DesignTime.Id, DesignTime.LoggerFactory, NullUnitService.Instance, null)
    {
        DesignTime.ThrowIfNotDesignMode();
        var start = new GeoPoint(55.75, 37.6173, 250.0); // Moscow coordinates
        var index = 0;
        var maxIndex = Enum.GetValues<AsvColorKind>().Length;
        Observable
            .Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2))
            .Subscribe(x =>
            {
                if (Random.Shared.NextDouble() > 0.9)
                {
                    IsNetworkError = true;
                    return;
                }

                var point = new GeoPoint(
                    start.Latitude + Random.Shared.NextDouble(),
                    start.Longitude + Random.Shared.NextDouble(),
                    start.Altitude + Random.Shared.NextDouble() + 0.5
                );
                Status = Enum.GetValues<AsvColorKind>()[index++ % maxIndex];
                ProgressStatus = Enum.GetValues<AsvColorKind>()[index++ % maxIndex];
                Progress = Random.Shared.NextDouble();
                StatusText = Status.ToString();
                GeoPointProperty.ModelValue.Value = point;
                Updated();
            });
        Header = "UAV position";
        ShortHeader = "UAV";
        Icon = MaterialIconKind.AddressMarker;
    }

    public GeoPointRttBoxViewModel(
        NavigationId id,
        ILoggerFactory loggerFactory,
        IUnitService units,
        TimeSpan? networkErrorTimeout
    )
        : base(id, loggerFactory, networkErrorTimeout)
    {
        _location = new ReactiveProperty<GeoPoint>(GeoPoint.NaN).DisposeItWith(Disposable);
        _latitudeUnit =
            units[LatitudeBase.Id]
            ?? throw new ArgumentException("Latitude unit not found in unit service");
        _longitudeUnit =
            units[LongitudeBase.Id]
            ?? throw new ArgumentException("Longitude unit not found in unit service");
        _altitudeUnit =
            units[AltitudeBase.Id]
            ?? throw new ArgumentException("Altitude unit not found in unit service");
        GeoPointProperty = new BindableGeoPointProperty(
            nameof(GeoPointProperty),
            _location,
            _latitudeUnit,
            _longitudeUnit,
            _altitudeUnit,
            loggerFactory,
            options =>
            {
                options.AltitudeFormat = "F2";
            }
        ).DisposeItWith(Disposable);
    }

    public BindableGeoPointProperty GeoPointProperty { get; }

    public string? StatusText
    {
        get;
        set => SetField(ref field, value);
    }

    public string? ShortStatusText
    {
        get;
        set => SetField(ref field, value);
    }
}

public class GeoPointRttBoxViewModel<T>
    : GeoPointRttBoxViewModel,
        IUpdatableRttBoxViewModel<GeoPointRttBoxViewModel<T>, T>
{
    private readonly TimeSpan? _networkErrorTimeout;

    public GeoPointRttBoxViewModel(
        NavigationId id,
        ILoggerFactory loggerFactory,
        IUnitService units,
        Observable<T> value,
        TimeSpan? networkErrorTimeout
    )
        : base(id, loggerFactory, units, networkErrorTimeout)
    {
        _networkErrorTimeout = networkErrorTimeout;
        value
            .ThrottleLastFrame(1)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(OnValueChanged)
            .DisposeItWith(Disposable);
    }

    public required Action<GeoPointRttBoxViewModel<T>, T> UpdateAction { get; init; }

    private void OnValueChanged(T value)
    {
        Debug.Assert(UpdateAction != null, "UpdateAction must be set");
        UpdateAction(this, value);
        if (_networkErrorTimeout is not null)
        {
            Updated();
        }
    }
}

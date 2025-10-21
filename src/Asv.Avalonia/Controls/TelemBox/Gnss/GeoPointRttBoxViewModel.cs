using System.Diagnostics;
using Asv.Common;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class GeoPointRttBoxViewModel : RttBoxViewModel
{
    private readonly IUnitItem _latitudeUnit;
    private readonly IUnitItem _longitudeUnit;
    private readonly IUnitItem _altitudeUnit;

    public GeoPointRttBoxViewModel()
    {
        Location = new ReactiveProperty<GeoPoint>(GeoPoint.NaN).DisposeItWith(Disposable);
        DesignTime.ThrowIfNotDesignMode();
        var start = new GeoPoint(55.75, 37.6173, 250.0); // Moscow coordinates
        var index = 0;
        var maxIndex = Enum.GetValues<RttBoxStatus>().Length;
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
                Status = Enum.GetValues<RttBoxStatus>()[index++ % maxIndex];
                ProgressStatus = Enum.GetValues<RttBoxStatus>()[index++ % maxIndex];
                Progress = Random.Shared.NextDouble();
                StatusText = Status.ToString();
                Location.OnNext(point);
                Updated();
            });
        _latitudeUnit = new DmsLatitudeUnit();
        _longitudeUnit = new DmsLongitudeUnit();
        _altitudeUnit = new MeterAltitudeUnit();
        Header = "UAV position";
        ShortHeader = "UAV";
        Icon = MaterialIconKind.AddressMarker;

        Location.Subscribe(OnValueChanged).DisposeItWith(Disposable);
    }

    public GeoPointRttBoxViewModel(
        NavigationId id,
        ILoggerFactory loggerFactory,
        IUnitService units,
        TimeSpan? networkErrorTimeout
    )
        : base(id, loggerFactory, networkErrorTimeout)
    {
        Location = new ReactiveProperty<GeoPoint>(GeoPoint.NaN).DisposeItWith(Disposable);
        _latitudeUnit =
            units[LatitudeBase.Id]?.CurrentUnitItem.CurrentValue
            ?? throw new ArgumentException("Latitude unit not found in unit service");
        _longitudeUnit =
            units[LongitudeBase.Id]?.CurrentUnitItem.CurrentValue
            ?? throw new ArgumentException("Longitude unit not found in unit service");
        _altitudeUnit =
            units[AltitudeBase.Id]?.CurrentUnitItem.CurrentValue
            ?? throw new ArgumentException("Altitude unit not found in unit service");

        Location.Subscribe(OnValueChanged).DisposeItWith(Disposable);
    }

    public ReactiveProperty<GeoPoint> Location { get; }

    private void OnValueChanged(GeoPoint geoPoint)
    {
        if (
            double.IsNaN(geoPoint.Latitude)
            || double.IsNaN(geoPoint.Longitude)
            || double.IsNaN(geoPoint.Altitude)
        )
        {
            LatitudeString = NotAvailableString;
            LongitudeString = NotAvailableString;
            AltitudeString = NotAvailableString;
        }
        else
        {
            LatitudeString = _latitudeUnit.PrintFromSi(geoPoint.Latitude);
            LongitudeString = _longitudeUnit.PrintFromSi(geoPoint.Longitude);
            AltitudeString = _altitudeUnit.PrintWithUnits(
                _altitudeUnit.FromSi(geoPoint.Altitude),
                "F2"
            );
        }
    }

    public string NotAvailableString { get; set; } = "N\\A";

    public string? AltitudeString
    {
        get;
        set => SetField(ref field, value);
    }

    public string? LongitudeString
    {
        get;
        set => SetField(ref field, value);
    }

    public string? LatitudeString
    {
        get;
        set => SetField(ref field, value);
    }

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

public class GeoPointRttBoxViewModel<T> : GeoPointRttBoxViewModel
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
        if (_networkErrorTimeout != null)
        {
            Updated();
        }
    }
}

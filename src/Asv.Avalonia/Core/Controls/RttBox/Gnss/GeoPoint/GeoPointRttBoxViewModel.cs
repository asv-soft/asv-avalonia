using System.Diagnostics;
using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class GeoPointRttBoxViewModel : RttBoxViewModel
{
    public GeoPointRttBoxViewModel()
        : this(DesignTime.Id.TypeId, DesignTime.LoggerFactory, NullUnitService.Instance, null)
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
        string typeId,
        ILoggerFactory loggerFactory,
        IUnitService units,
        TimeSpan? networkErrorTimeout
    )
        : base(typeId, networkErrorTimeout)
    {
        var location = new ReactiveProperty<GeoPoint>(GeoPoint.NaN).DisposeItWith(Disposable);
        GeoPointProperty = new BindableGeoPointProperty(
            "geo-point",
            location,
            units,
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
        string typeId,
        ILoggerFactory loggerFactory,
        IUnitService units,
        Observable<T> value,
        TimeSpan? networkErrorTimeout
    )
        : base(typeId, loggerFactory, units, networkErrorTimeout)
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

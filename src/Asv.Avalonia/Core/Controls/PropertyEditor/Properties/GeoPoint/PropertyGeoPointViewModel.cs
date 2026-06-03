using Asv.Common;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public abstract class PropertyGeoPointViewModel : PropertyViewModel
{
    private readonly ReactiveProperty<double> _altitude;
    private readonly ReactiveProperty<double> _latitude;
    private readonly ReactiveProperty<double> _longitude;
    private bool _updatingFromModel;

    protected PropertyGeoPointViewModel(string id, IUnitService unitService)
        : base(id)
    {
        ArgumentNullException.ThrowIfNull(unitService);

        _latitude = new ReactiveProperty<double>().AddTo(ref DisposableBag);
        _longitude = new ReactiveProperty<double>().AddTo(ref DisposableBag);
        _altitude = new ReactiveProperty<double>().AddTo(ref DisposableBag);

        _latitude.Skip(1).Subscribe(_ => ApplyValueFromComponents()).AddTo(ref DisposableBag);
        _longitude.Skip(1).Subscribe(_ => ApplyValueFromComponents()).AddTo(ref DisposableBag);
        _altitude.Skip(1).Subscribe(_ => ApplyValueFromComponents()).AddTo(ref DisposableBag);

        Latitude = new PropertyUnitReactive(
            nameof(Latitude),
            unitService[LatitudeUnit.Id] ?? throw new NullReferenceException("Latitude Unit"),
            _latitude,
            format: "F7"
        )
        {
            ShortHeader = RS.Latitude_ShortName,
            Icon = MaterialIconKind.Latitude,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        Longitude = new PropertyUnitReactive(
            nameof(Longitude),
            unitService[LongitudeUnit.Id] ?? throw new NullReferenceException("Longitude Unit"),
            _longitude,
            format: "F7"
        )
        {
            ShortHeader = RS.Longitude_ShortName,
            Icon = MaterialIconKind.Longitude,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        Altitude = new PropertyUnitReactive(
            nameof(Altitude),
            unitService[AltitudeUnit.Id] ?? throw new NullReferenceException("Altitude Unit"),
            _altitude,
            format: "F2"
        )
        {
            ShortHeader = RS.Altitude_ShortName,
            Icon = MaterialIconKind.Altimeter,
        }
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
    }

    public PropertyUnitViewModel Latitude { get; }
    public PropertyUnitViewModel Longitude { get; }
    public PropertyUnitViewModel Altitude { get; }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Latitude;
        yield return Longitude;
        yield return Altitude;
        foreach (var item in base.GetChildren())
        {
            yield return item;
        }
    }

    protected virtual void ApplyValueFromModel(GeoPoint value)
    {
        _updatingFromModel = true;
        try
        {
            _latitude.Value = value.Latitude;
            _longitude.Value = value.Longitude;
            _altitude.Value = value.Altitude;
        }
        finally
        {
            _updatingFromModel = false;
        }
    }

    protected abstract ValueTask ApplyFromUser(GeoPoint value, CancellationToken cancel);

    private void ApplyValueFromComponents()
    {
        if (_updatingFromModel)
        {
            return;
        }

        _ = ApplyFromUser(
            new GeoPoint(_latitude.Value, _longitude.Value, _altitude.Value),
            CancellationToken.None
        );
    }
}

using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class BindableGeoPointProperty : CompositeBindablePropertyBase<GeoPoint>
{
    protected readonly ReactiveProperty<double> ModelLat;
    protected readonly ReactiveProperty<double> ModelAlt;
    protected readonly ReactiveProperty<double> ModelLon;

    public BindableGeoPointProperty(
        NavigationId id,
        ReactiveProperty<GeoPoint> modelValue,
        IUnit latUnit,
        IUnit lonUnit,
        IUnit altUnit,
        ILoggerFactory loggerFactory,
        IRoutable parent
    )
        : base(id, loggerFactory, parent)
    {
        ModelValue = modelValue;

        ModelLat = new ReactiveProperty<double>(modelValue.CurrentValue.Latitude).DisposeItWith(
            Disposable
        );

        ModelLat
            .Subscribe(x =>
            {
                ModelValue.Value = new GeoPoint(
                    x,
                    ModelValue.Value.Longitude,
                    ModelValue.Value.Altitude
                );
            })
            .DisposeItWith(Disposable);

        ModelLon = new ReactiveProperty<double>(modelValue.CurrentValue.Longitude).DisposeItWith(
            Disposable
        );
        ModelLon
            .Subscribe(x =>
            {
                ModelValue.Value = new GeoPoint(
                    ModelValue.Value.Latitude,
                    x,
                    ModelValue.Value.Altitude
                );
            })
            .DisposeItWith(Disposable);

        ModelAlt = new ReactiveProperty<double>(modelValue.CurrentValue.Altitude).DisposeItWith(
            Disposable
        );
        ModelAlt
            .Subscribe(x =>
            {
                ModelValue.Value = new GeoPoint(
                    ModelValue.Value.Latitude,
                    ModelValue.Value.Longitude,
                    x
                );
            })
            .DisposeItWith(Disposable);

        Latitude = new BindableUnitProperty(
            nameof(Latitude),
            ModelLat,
            latUnit,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        Longitude = new BindableUnitProperty(
            nameof(Longitude),
            ModelLon,
            lonUnit,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);
        Altitude = new BindableUnitProperty(
            nameof(Altitude),
            ModelAlt,
            altUnit,
            loggerFactory,
            this
        ).DisposeItWith(Disposable);

        ModelValue
            .Subscribe(x =>
            {
                ModelLat.Value = x.Latitude;
                ModelLon.Value = x.Longitude;
                ModelAlt.Value = x.Altitude;
            })
            .DisposeItWith(Disposable);
    }

    public BindableUnitProperty Latitude { get; }
    public BindableUnitProperty Longitude { get; }
    public BindableUnitProperty Altitude { get; }

    public override void ForceValidate()
    {
        Latitude.ForceValidate();
        Longitude.ForceValidate();
        Altitude.ForceValidate();
    }

    public override IEnumerable<IRoutable> GetRoutableChildren()
    {
        yield return Latitude;
        yield return Longitude;
        yield return Altitude;
    }

    public sealed override ReactiveProperty<GeoPoint> ModelValue { get; }
}

using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class HistoricalGeoPointProperty : BindableGeoPointProperty, IHistoricalProperty<GeoPoint>
{
    public HistoricalGeoPointProperty(
        NavigationId id,
        ReactiveProperty<GeoPoint> modelValue,
        IUnit latUnit,
        IUnit lonUnit,
        IUnit altUnit,
        ILoggerFactory loggerFactory
    )
        : base(id, modelValue, latUnit, lonUnit, altUnit, loggerFactory)
    {
        base.Latitude.Dispose();
        base.Longitude.Dispose();
        base.Altitude.Dispose();

        Latitude = new HistoricalUnitProperty(nameof(Latitude), ModelLat, latUnit, loggerFactory)
        {
            Parent = this,
        }.DisposeItWith(Disposable);
        Longitude = new HistoricalUnitProperty(nameof(Longitude), ModelLon, lonUnit, loggerFactory)
        {
            Parent = this,
        }.DisposeItWith(Disposable);
        Altitude = new HistoricalUnitProperty(nameof(Altitude), ModelAlt, altUnit, loggerFactory)
        {
            Parent = this,
        }.DisposeItWith(Disposable);
    }

    public new HistoricalUnitProperty Latitude { get; }
    public new HistoricalUnitProperty Longitude { get; }
    public new HistoricalUnitProperty Altitude { get; }

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
}

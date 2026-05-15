using Asv.Common;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia;

public class HistoricalGeoPointProperty : BindableGeoPointProperty, IHistoricalProperty<GeoPoint>
{
    public HistoricalGeoPointProperty(
        string typeId,
        ReactiveProperty<GeoPoint> modelValue,
        IUnitService unitService,
        ILoggerFactory loggerFactory,
        Action<GeoPointPropertyOptions>? configureOptions = null
    )
        : base(typeId, modelValue, unitService, loggerFactory, configureOptions)
    {
        base.Latitude.Dispose();
        base.Longitude.Dispose();
        base.Altitude.Dispose();

        var latUnit = unitService.GetRequiredUnitOfType<LatitudeUnit>(LatitudeUnit.Id);
        var lonUnit = unitService.GetRequiredUnitOfType<LongitudeUnit>(LongitudeUnit.Id);
        var altUnit = unitService.GetRequiredUnitOfType<AltitudeUnit>(AltitudeUnit.Id);

        Latitude = new HistoricalUnitProperty<LatitudeUnit>(
            nameof(Latitude),
            ModelLat,
            latUnit,
            loggerFactory,
            Options.LatitudeFormat
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        Longitude = new HistoricalUnitProperty<LongitudeUnit>(
            nameof(Longitude),
            ModelLon,
            lonUnit,
            loggerFactory,
            Options.LongitudeFormat
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
        Altitude = new HistoricalUnitProperty<AltitudeUnit>(
            nameof(Altitude),
            ModelAlt,
            altUnit,
            loggerFactory,
            Options.AltitudeFormat
        )
            .SetRoutableParent(this)
            .DisposeItWith(Disposable);
    }

    public new HistoricalUnitProperty<LatitudeUnit> Latitude { get; }
    public new HistoricalUnitProperty<LongitudeUnit> Longitude { get; }
    public new HistoricalUnitProperty<AltitudeUnit> Altitude { get; }

    public override void ForceValidate()
    {
        Latitude.ForceValidate();
        Longitude.ForceValidate();
        Altitude.ForceValidate();
    }

    public override IEnumerable<IViewModel> GetChildren()
    {
        yield return Latitude;
        yield return Longitude;
        yield return Altitude;
    }
}

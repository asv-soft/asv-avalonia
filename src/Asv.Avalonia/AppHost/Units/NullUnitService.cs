namespace Asv.Avalonia;

public class NullUnitService : IUnitService
{
    #region Static

    public static NullUnitService Instance { get; } = new();

    #endregion
    private readonly Lock _lock = new();

    private readonly IUnit _altitude = new AltitudeUnit(
        DesignTime.Configuration,
        [new AltitudeMeterUnitItem(), new AltitudeFeetUnitItem()]
    );
    private readonly IUnit _latitude = new LatitudeUnit(
        DesignTime.Configuration,
        [new LatitudeDmsUnitItem(), new LatitudeDegreeUnitItem()]
    );
    private readonly IUnit _longitude = new LongitudeUnit(
        DesignTime.Configuration,
        [new LongitudeDmsUnitItem(), new LongitudeDegreeUnitItem()]
    );
    private readonly IUnit _angle = new AngleUnit(
        DesignTime.Configuration,
        [new AngleDegreeUnitItem(), new AngleDmsUnitItem()]
    );
    private readonly IUnit _frequency = new FrequencyUnit(
        DesignTime.Configuration,
        [
            new FrequencyHertzUnitItem(),
            new FrequencyGigahertzUnitItem(),
            new FrequencyMegahertzUnitItem(),
            new FrequencyKilohertzUnitItem(),
        ]
    );
    private readonly IUnit _meter = new DistanceUnit(
        DesignTime.Configuration,
        [new DistanceMeterUnitItem(), new DistanceNauticalMileUnitItem()]
    );

    private readonly Dictionary<string, IUnit> _units;

    private NullUnitService()
    {
        _units = new Dictionary<string, IUnit>([
            new(_altitude.UnitId, _altitude),
            new(_latitude.UnitId, _latitude),
            new(_longitude.UnitId, _longitude),
            new(_angle.UnitId, _angle),
            new(_frequency.UnitId, _frequency),
            new(_meter.UnitId, _meter),
        ]);
    }

    public IReadOnlyDictionary<string, IUnit> Units => _units;

    public void Extend(IUnit unit)
    {
        using (_lock.EnterScope())
        {
            if (Units.ContainsKey(unit.UnitId))
            {
                return;
            }

            _units.TryAdd(unit.UnitId, unit);
        }
    }
}

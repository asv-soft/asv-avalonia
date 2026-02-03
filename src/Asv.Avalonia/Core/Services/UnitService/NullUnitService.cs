namespace Asv.Avalonia;

public class NullUnitService : IUnitService
{
    #region Static

    public static NullUnitService Instance { get; } = new();

    #endregion

    private readonly IUnit _altitude = new AltitudeBase(
        DesignTime.Configuration,
        [new MeterAltitudeUnit(), new FeetAltitudeUnit()]
    );
    private readonly IUnit _latitude = new LatitudeBase(
        DesignTime.Configuration,
        [new DmsLatitudeUnit(), new DegreeLatitudeUnit()]
    );
    private readonly IUnit _longitude = new LongitudeBase(
        DesignTime.Configuration,
        [new DmsLongitudeUnit(), new DegreeLongitudeUnit()]
    );
    private readonly IUnit _angle = new AngleBase(
        DesignTime.Configuration,
        [new DegreeAngleUnit(), new DmsAngleUnit()]
    );
    private readonly IUnit _frequency = new FrequencyBase(
        DesignTime.Configuration,
        [
            new HertzFrequencyUnit(),
            new GigahertzFrequencyUnit(),
            new MegahertzFrequencyUnit(),
            new KilohertzFrequencyUnit(),
        ]
    );
    private readonly IUnit _meter = new DistanceBase(
        DesignTime.Configuration,
        [new MeterDistanceUnit(), new NauticalMileDistanceUnit()]
    );

    private NullUnitService()
    {
        Units = new Dictionary<string, IUnit>(
            new KeyValuePair<string, IUnit>[]
            {
                new(_altitude.UnitId, _altitude),
                new(_latitude.UnitId, _latitude),
                new(_longitude.UnitId, _longitude),
                new(_angle.UnitId, _angle),
                new(_frequency.UnitId, _frequency),
                new(_meter.UnitId, _meter),
            }
        );
    }

    public IReadOnlyDictionary<string, IUnit> Units { get; }
}

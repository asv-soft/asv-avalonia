namespace Asv.Avalonia;

public class NullUnitService : IUnitService
{
    #region Static

    public static NullUnitService Instance { get; } = new();

    #endregion

    private readonly IUnit _altitude = new AltitudeBase(DesignTime.Configuration, [new MeterAltitudeUnit(), new FeetAltitudeUnit()]);
    private readonly IUnit _latitude = new LatitudeBase(DesignTime.Configuration, [new DmsLatitudeUnit(), new DegreeLatitudeUnit()]);
    private readonly IUnit _longitude = new LongitudeBase(DesignTime.Configuration, [new DmsLongitudeUnit(), new DegreeLongitudeUnit()]);
    private readonly IUnit _angle = new AngleBase(DesignTime.Configuration, [new DegreeAngleUnit(), new DmsAngleUnit()]);
    
    private NullUnitService()
    {
        Units = new Dictionary<string, IUnit>(new KeyValuePair<string, IUnit>[] {
            new(_altitude.UnitId, _altitude),
            new(_latitude.UnitId, _latitude),
            new(_longitude.UnitId, _longitude),
            new(_angle.UnitId, _angle),
            }
        );
    }

    
    
    public IReadOnlyDictionary<string, IUnit> Units { get; }
}

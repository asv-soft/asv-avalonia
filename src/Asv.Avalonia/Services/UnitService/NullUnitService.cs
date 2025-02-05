using ObservableCollections;

namespace Asv.Avalonia;

public class NullUnitService : UnitService
{
    #region Static

    public static NullUnitService Instance { get; } = new();

    #endregion

    private NullUnitService()
        : base([new Distance([new MeterDistanceUnit()])]) { }
}

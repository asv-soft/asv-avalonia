using Material.Icons;
using R3;

namespace Asv.Avalonia;

public interface IUnit : IDisposable
{
    string Name { get; }
    string Description { get; }
    string UnitId { get; }
    IReadOnlyDictionary<string, IUnitItem> AvailableUnits { get; }
    ReactiveProperty<IUnitItem> Current { get; }
    IUnitItem InternationalSystemUnit { get; }
    public MaterialIconKind Icon { get; }
    IUnitItem this[string unitItemId] =>
        AvailableUnits.GetValueOrDefault(unitItemId) ?? InternationalSystemUnit;
}

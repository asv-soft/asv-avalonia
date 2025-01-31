using System.Collections.Immutable;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public abstract class UnitBase : IUnit
{
    private readonly ImmutableDictionary<string, IUnitItem> _items;

    protected UnitBase(IEnumerable<IUnitItem> items)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, IUnitItem>();
        foreach (var item in items)
        {
            if (builder.TryAdd(item.UnitItemId, item) == false)
            {
                throw new InvalidOperationException($"Duplicate unit item id {item.UnitItemId}");
            }
        }

        _items = builder.ToImmutable();
        var defaultUnit = _items.Where(x => x.Value.IsInternationalSystemUnit).ToArray();
        if (defaultUnit.Length != 1)
        {
            throw new InvalidOperationException("There must be exactly one default (SI) unit");
        }

        InternationalSystemUnit = defaultUnit[0].Value;
        Current = new ReactiveProperty<IUnitItem>(InternationalSystemUnit);
    }

    public IReadOnlyDictionary<string, IUnitItem> AvailableUnits => _items;
    public abstract MaterialIconKind Icon { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string UnitId { get; }
    public ReadOnlyReactiveProperty<IUnitItem> Current { get; }
    public IUnitItem InternationalSystemUnit { get; }

    public void Dispose()
    {
        Current.Dispose();
    }
}
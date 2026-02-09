using System.Collections.Immutable;
using System.Composition;

namespace Asv.Avalonia;

public class UnitService : IUnitService
{
    private readonly ImmutableSortedDictionary<string, IUnit> _units;

    public UnitService([ImportMany] IEnumerable<IUnit> items)
    {
        var builder = ImmutableSortedDictionary.CreateBuilder<string, IUnit>();
        foreach (var item in items.OrderBy(x => x.Name))
        {
            if (builder.TryAdd(item.UnitId, item) == false)
            {
                throw new InvalidOperationException($"Duplicate unit id {item.UnitId}");
            }
        }

        _units = builder.ToImmutable();
    }

    public IReadOnlyDictionary<string, IUnit> Units => _units;
}

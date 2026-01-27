using System.Collections.Immutable;
using System.Composition;

namespace Asv.Avalonia;

[Export(typeof(IUnitService))]
[Shared]
public class UnitService : IUnitService
{
    private readonly ImmutableSortedDictionary<string, IUnit> _units;

    [ImportingConstructor]
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

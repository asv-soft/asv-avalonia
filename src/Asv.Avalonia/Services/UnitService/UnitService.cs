using System.Collections.Immutable;
using System.Composition;
using ObservableCollections;

namespace Asv.Avalonia;

[Export(typeof(IUnitService))]
[Shared]
public class UnitService : IUnitService
{
    private readonly ImmutableDictionary<string, IUnit> _units;

    [ImportingConstructor]
    public UnitService([ImportMany] IEnumerable<IUnit> items)
    {
        var builder = ImmutableDictionary.CreateBuilder<string, IUnit>();
        foreach (var item in items)
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
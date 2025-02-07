using System.Collections.Immutable;
using Asv.Cfg;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public class UnitBaseConfig
{
    public IDictionary<string, string?> CurrentUnitItems { get; set; } =
        new Dictionary<string, string?>();
}

public abstract class UnitBase : IUnit
{
    private readonly ImmutableDictionary<string, IUnitItem> _items;
    private readonly IConfiguration _cfgSvc;
    private readonly UnitBaseConfig _config;

    protected UnitBase(IConfiguration cfgSvc, IEnumerable<IUnitItem> items)
    {
        ArgumentNullException.ThrowIfNull(cfgSvc);
        _cfgSvc = cfgSvc;
        _config = cfgSvc.Get<UnitBaseConfig>();
        var builder = ImmutableDictionary.CreateBuilder<string, IUnitItem>();
        foreach (var item in items)
        {
            if (!builder.TryAdd(item.UnitItemId, item))
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
        _config.CurrentUnitItems.TryGetValue(UnitId, out var currentUnitItemId);
        if (currentUnitItemId is not null)
        {
            _items.TryGetValue(currentUnitItemId, out var current);

            if (current is not null)
            {
                Current.Value = current;
            }
        }

        _sub1 = Current.Subscribe(SetUnitItem);
    }

    private void SetUnitItem(IUnitItem unitItem)
    {
        _config.CurrentUnitItems.TryGetValue(UnitId, out var currentUnitItemId);
        if (currentUnitItemId == unitItem.UnitItemId)
        {
            return;
        }

        _config.CurrentUnitItems[UnitId] = unitItem.UnitItemId;
        _cfgSvc.Set(_config);
    }

    public IReadOnlyDictionary<string, IUnitItem> AvailableUnits => _items;
    public abstract MaterialIconKind Icon { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string UnitId { get; }
    public ReactiveProperty<IUnitItem> Current { get; }
    public IUnitItem InternationalSystemUnit { get; }

    #region Dispose

    private readonly IDisposable _sub1;

    public void Dispose()
    {
        _sub1.Dispose();
        Current.Dispose();
    }

    #endregion
}

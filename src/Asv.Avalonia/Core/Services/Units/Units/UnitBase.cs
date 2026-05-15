using System.Collections.Immutable;
using Asv.Cfg;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public interface IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public abstract class UnitBase<TConfig> : IUnit
    where TConfig : class, IUnitConfig, new()
{
    private readonly ImmutableDictionary<string, IUnitItem> _items;
    private readonly IConfiguration _cfgSvc;
    private readonly TConfig _config;

    protected UnitBase(IConfiguration cfgSvc, IEnumerable<IUnitItem> items)
    {
        ArgumentNullException.ThrowIfNull(cfgSvc);
        _cfgSvc = cfgSvc;

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

        var defaultUnitItem = InternationalSystemUnit;

        _config = cfgSvc.Get<TConfig>();
        if (_config.CurrentUnitItemId is not null)
        {
            if (_items.TryGetValue(_config.CurrentUnitItemId, out var unit))
            {
                defaultUnitItem = unit;
            }
        }

        CurrentUnitItem = new SynchronizedReactiveProperty<IUnitItem>(defaultUnitItem);

        _sub1 = CurrentUnitItem.Subscribe(SetUnitItem);
    }

    private void SetUnitItem(IUnitItem unitItem)
    {
        if (_config.CurrentUnitItemId == unitItem.UnitItemId)
        {
            return;
        }

        _config.CurrentUnitItemId = unitItem.UnitItemId;
        _cfgSvc.Set(_config);
    }

    public IReadOnlyDictionary<string, IUnitItem> AvailableUnits => _items;
    public abstract MaterialIconKind Icon { get; }

    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string UnitId { get; }
    public SynchronizedReactiveProperty<IUnitItem> CurrentUnitItem { get; }
    public IUnitItem InternationalSystemUnit { get; }

    #region Dispose

    private readonly IDisposable _sub1;

    public void Dispose()
    {
        _sub1.Dispose();
        CurrentUnitItem.Dispose();
    }

    #endregion
}

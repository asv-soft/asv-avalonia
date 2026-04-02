using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

internal sealed class CapacityConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public class CapacityUnit : UnitBase
{
    public const string Id = "capacity";
    private readonly CapacityConfig? _config;
    private readonly IConfiguration _cfgSvc;

    public CapacityUnit(IConfiguration cfgSvc, [FromKeyedServices(Id)] IEnumerable<IUnitItem> items)
        : base(items)
    {
        ArgumentNullException.ThrowIfNull(cfgSvc);
        _cfgSvc = cfgSvc;
        _config = cfgSvc.Get<CapacityConfig>();
        if (_config.CurrentUnitItemId is null)
        {
            return;
        }

        AvailableUnits.TryGetValue(_config.CurrentUnitItemId, out var unit);
        if (unit is not null)
        {
            CurrentUnitItem.OnNext(unit);
        }
    }

    protected override void SetUnitItem(IUnitItem unitItem)
    {
        if (_config is null)
        {
            return;
        }

        if (_config.CurrentUnitItemId == unitItem.UnitItemId)
        {
            return;
        }

        _config.CurrentUnitItemId = unitItem.UnitItemId;
        _cfgSvc.Set(_config);
    }

    public override MaterialIconKind Icon => MaterialIconKind.Battery;
    public override string Name => RS.Mah_UnitItem_Name;
    public override string Description => RS.Capacity_UnitItem_Description;
    public override string UnitId => Id;
}

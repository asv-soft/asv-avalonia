using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class CapacityConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class CapacityUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(CapacityUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<CapacityConfig>(cfgSvc, items)
{
    public const string Id = "capacity";

    public override MaterialIconKind Icon => MaterialIconKind.Battery;
    public override string Name => RS.Capacity_Name;
    public override string Description => RS.Capacity_UnitItem_Description;
    public override string UnitId => Id;
}

using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class DistanceConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class DistanceUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(DistanceUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<DistanceConfig>(cfgSvc, items)
{
    public const string Id = "distance";

    public override MaterialIconKind Icon => MaterialIconKind.LocationDistance;
    public override string Name => RS.Distance_Name;
    public override string Description => RS.Distance_Description;
    public override string UnitId => Id;
}

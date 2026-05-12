using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class BearingConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class BearingUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(BearingUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<BearingConfig>(cfgSvc, items)
{
    public const string Id = "bearing";

    public override MaterialIconKind Icon => MaterialIconKind.Location;
    public override string Name => RS.Bearing_Name;
    public override string Description => RS.Bearing_Description;
    public override string UnitId => Id;
}

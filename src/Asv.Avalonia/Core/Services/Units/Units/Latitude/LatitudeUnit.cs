using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class LatitudeConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class LatitudeUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(LatitudeUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<LatitudeConfig>(cfgSvc, items)
{
    public const string Id = "latitude";

    public override MaterialIconKind Icon => MaterialIconKind.Latitude;
    public override string Name => RS.Latitude_Name;
    public override string Description => RS.Latitude_Description;
    public override string UnitId => Id;
}

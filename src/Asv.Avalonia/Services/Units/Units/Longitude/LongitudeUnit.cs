using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class LongitudeConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class LongitudeUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(LongitudeUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<LongitudeConfig>(cfgSvc, items)
{
    public const string Id = "longitude";

    public override MaterialIconKind Icon => MaterialIconKind.Longitude;
    public override string Name => RS.Longitude_Name;
    public override string Description => RS.Longitude_Description;
    public override string UnitId => Id;
}

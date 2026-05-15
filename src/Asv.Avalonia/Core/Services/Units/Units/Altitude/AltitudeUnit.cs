using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class AltitudeConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class AltitudeUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(AltitudeUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<AltitudeConfig>(cfgSvc, items)
{
    public const string Id = "altitude";

    public override MaterialIconKind Icon => MaterialIconKind.ArrowUpward;
    public override string Name => RS.Altitude_Name;
    public override string Description => RS.Altitude_Description;
    public override string UnitId => Id;
}

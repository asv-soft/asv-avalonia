using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class PowerConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class PowerUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(PowerUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<PowerConfig>(cfgSvc, items)
{
    public const string Id = "power";

    public override MaterialIconKind Icon => MaterialIconKind.LightningBolt;
    public override string Name => RS.Power_Name;
    public override string Description => RS.Power_Description;
    public override string UnitId => Id;
}

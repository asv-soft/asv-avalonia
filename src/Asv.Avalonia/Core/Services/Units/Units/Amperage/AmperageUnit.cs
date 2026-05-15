using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class AmperageConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public class AmperageUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(AmperageUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<AmperageConfig>(cfgSvc, items)
{
    public const string Id = "amperage";

    public override MaterialIconKind Icon => MaterialIconKind.Electricity;
    public override string Name => RS.Amperage_Name;
    public override string Description => RS.Amperage_Description;
    public override string UnitId => Id;
}

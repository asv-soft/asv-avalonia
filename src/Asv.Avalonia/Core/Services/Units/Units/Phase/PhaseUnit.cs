using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class PhaseConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class PhaseUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(PhaseUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<PhaseConfig>(cfgSvc, items)
{
    public const string Id = "phase";

    public override MaterialIconKind Icon => MaterialIconKind.PowerPlug;
    public override string Name => RS.Phase_Name;
    public override string Description => RS.Phase_Description;
    public override string UnitId => Id;
}

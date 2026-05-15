using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class VelocityConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class VelocityUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(VelocityUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<VelocityConfig>(cfgSvc, items)
{
    public const string Id = "velocity";

    public override MaterialIconKind Icon => MaterialIconKind.Velocity;
    public override string Name => RS.Velocity_Name;
    public override string Description => RS.Velocity_Description;
    public override string UnitId => Id;
}

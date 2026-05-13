using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class ThrottleConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class ThrottleUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(ThrottleUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<ThrottleConfig>(cfgSvc, items)
{
    public const string Id = "throttle";

    public override MaterialIconKind Icon => MaterialIconKind.Timelapse;
    public override string Name => RS.Throttle_Name;
    public override string Description => RS.Throttle_Description;
    public override string UnitId => Id;
}

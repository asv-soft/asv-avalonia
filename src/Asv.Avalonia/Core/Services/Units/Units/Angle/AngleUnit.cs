using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class DegreeConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class AngleUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(AngleUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<DegreeConfig>(cfgSvc, items)
{
    public const string Id = "angle";

    public override MaterialIconKind Icon => MaterialIconKind.AngleRight;
    public override string Name => RS.Angle_Name;
    public override string Description => RS.Angle_Description;
    public override string UnitId => Id;
}

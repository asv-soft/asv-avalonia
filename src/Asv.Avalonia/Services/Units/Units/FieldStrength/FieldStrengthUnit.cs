using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class FieldStrengthConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class FieldStrengthUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(FieldStrengthUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<FieldStrengthConfig>(cfgSvc, items)
{
    public const string Id = "field.strength";

    public override MaterialIconKind Icon => MaterialIconKind.HighVoltage;
    public override string Name => RS.FieldStrength_Name;
    public override string Description => RS.FieldStrength_Description;
    public override string UnitId => Id;
}

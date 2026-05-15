using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class VoltageConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class VoltageUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(VoltageUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<VoltageConfig>(cfgSvc, items)
{
    public const string Id = "voltage";

    public override MaterialIconKind Icon => MaterialIconKind.HighVoltage;
    public override string Name => RS.Voltage_Name;
    public override string Description => RS.Voltage_Description;
    public override string UnitId => Id;
}

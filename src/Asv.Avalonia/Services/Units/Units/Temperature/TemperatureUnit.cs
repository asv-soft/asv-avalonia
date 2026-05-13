using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class TemperatureConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class TemperatureUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(TemperatureUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<TemperatureConfig>(cfgSvc, items)
{
    public const string Id = "temperature";

    public override MaterialIconKind Icon => MaterialIconKind.Temperature;
    public override string Name => RS.Temperature_Name;
    public override string Description => RS.Temperature_Description;
    public override string UnitId => Id;
}

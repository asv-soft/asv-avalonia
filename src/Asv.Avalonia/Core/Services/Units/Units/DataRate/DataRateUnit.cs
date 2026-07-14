using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class DataRateConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class DataRateUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(DataRateUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<DataRateConfig>(cfgSvc, items)
{
    public const string Id = "data_rate";
    public const double ScaleFactor = 1000.0;

    public override MaterialIconKind Icon => MaterialIconKind.Speedometer;
    public override string Name => RS.DataRate_Name;
    public override string Description => RS.DataRate_Description;
    public override string UnitId => Id;
}

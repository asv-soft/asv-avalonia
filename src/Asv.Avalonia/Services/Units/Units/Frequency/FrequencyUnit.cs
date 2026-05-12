using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class FrequencyConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class FrequencyUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(FrequencyUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<FrequencyConfig>(cfgSvc, items)
{
    public const string Id = "frequency";

    public override MaterialIconKind Icon => MaterialIconKind.Frequency;
    public override string Name => RS.Frequency_Name;
    public override string Description => RS.Frequency_Description;
    public override string UnitId => Id;
}

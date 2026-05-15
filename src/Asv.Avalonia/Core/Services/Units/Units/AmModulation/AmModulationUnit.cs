using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class AmplitudeModulationConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class AmModulationUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(AmModulationUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<AmplitudeModulationConfig>(cfgSvc, items)
{
    public const string Id = "amplitude.modulation";

    public override MaterialIconKind Icon => MaterialIconKind.Amplitude;
    public override string Name => RS.AmplitudeModulation_Name;
    public override string Description => RS.AmplitudeModulation_Description;
    public override string UnitId => Id;
}

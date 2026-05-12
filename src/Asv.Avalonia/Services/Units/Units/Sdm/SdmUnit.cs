using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class SdmConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class SdmUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(SdmUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<SdmConfig>(cfgSvc, items)
{
    public const string Id = "sdm";

    public override MaterialIconKind Icon => MaterialIconKind.Amplitude;
    public override string Name => RS.Sdm_Name;
    public override string Description => RS.Sdm_Description;
    public override string UnitId => Id;
}

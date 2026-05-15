using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class DdmGpConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class DdmGpUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(DdmGpUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<DdmGpConfig>(cfgSvc, items)
{
    public const string Id = "dbm.gp";

    public override MaterialIconKind Icon => MaterialIconKind.Frequency;
    public override string Name => RS.DdmGp_Name;
    public override string Description => RS.DdmGp_Description;
    public override string UnitId => Id;
}

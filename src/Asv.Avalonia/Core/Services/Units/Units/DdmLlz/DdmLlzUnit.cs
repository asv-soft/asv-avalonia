using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class DdmLlzConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class DdmLlzUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(DdmLlzUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<DdmLlzConfig>(cfgSvc, items)
{
    public const string Id = "dbm.llz";

    public override MaterialIconKind Icon => MaterialIconKind.Frequency;
    public override string Name => RS.DdmLlz_Name;
    public override string Description => RS.DdmLlz_Description;
    public override string UnitId => Id;
}

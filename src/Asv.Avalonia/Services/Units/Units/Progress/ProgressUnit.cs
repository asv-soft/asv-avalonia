using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class ProgressConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class ProgressUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(ProgressUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<ProgressConfig>(cfgSvc, items)
{
    public const string Id = "progress";

    public override MaterialIconKind Icon => MaterialIconKind.ProgressDownload;
    public override string Name => RS.Progress_Name;
    public override string Description => RS.Progress_Description;
    public override string UnitId => Id;
}

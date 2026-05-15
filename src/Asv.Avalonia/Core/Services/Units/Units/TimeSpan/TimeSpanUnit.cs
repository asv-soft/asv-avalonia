using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class TimeSpanConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class TimeSpanUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(TimeSpanUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<TimeSpanConfig>(cfgSvc, items)
{
    public const string Id = "timespan";

    public override MaterialIconKind Icon => MaterialIconKind.Timelapse;
    public override string Name => RS.TimeSpan_Name;
    public override string Description => RS.TimeSpan_Description;
    public override string UnitId => Id;
}

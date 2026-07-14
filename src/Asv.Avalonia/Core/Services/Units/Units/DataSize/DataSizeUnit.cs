using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public sealed class DataSizeConfig : IUnitConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class DataSizeUnit(
    IConfiguration cfgSvc,
    [FromKeyedServices(DataSizeUnit.Id)] IEnumerable<IUnitItem> items
) : UnitBase<DataSizeConfig>(cfgSvc, items)
{
    public const string Id = "data_size";
    public const double ScaleFactor = 1024.0;

    public override MaterialIconKind Icon => MaterialIconKind.FileOutline;
    public override string Name => RS.DataSize_Name;
    public override string Description => RS.DataSize_Description;
    public override string UnitId => Id;
}

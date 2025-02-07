using System.Composition;
using Asv.Cfg;
using Material.Icons;

namespace Asv.Avalonia;

[ExportUnit]
[Shared]
[method: ImportingConstructor]
public class DistanceBase(
    [Import] IConfiguration cfgSvc,
    [ImportMany(DistanceBase.Id)] IEnumerable<IUnitItem> items
) : UnitBase(cfgSvc, items)
{
    public const string Id = "distance";

    public override MaterialIconKind Icon => MaterialIconKind.LocationDistance;
    public override string Name => RS.Distance_Name;
    public override string Description => RS.Distance_Description;
    public override string UnitId => Id;
}

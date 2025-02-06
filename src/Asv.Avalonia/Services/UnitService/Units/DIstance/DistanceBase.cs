using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

public interface ISourceInfo { }

[ExportUnit]
[Shared]
[method: ImportingConstructor]
public class DistanceBase([ImportMany(DistanceBase.Id)] IEnumerable<IUnitItem> items) : UnitBase(items)
{
    public const string Id = "distance";

    public override MaterialIconKind Icon => MaterialIconKind.LocationDistance;
    public override string Name => RS.Distance_Name;
    public override string Description => RS.Distance_Description;
    public override string UnitId => Id;
}

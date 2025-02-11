using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

public interface ISourceInfo { }

[ExportUnit]
[Shared]
[method: ImportingConstructor]
public class Distance([ImportMany(Distance.Id)] IEnumerable<IUnitItem> items) : UnitBase(items)
{
    public const string Id = "distance";

    public override MaterialIconKind Icon => MaterialIconKind.LocationDistance;
    public override string Name => RS.Distance_Name;
    public override string Description => RS.Distance_Description;
    public override string UnitId => Id;
}

[ExportUnitItem(Distance.Id)]
[Shared]
[method: ImportingConstructor]
public class MeterDistanceUnit() : UnitItemBase(1.0)
{
    public const string Id = $"{Distance.Id}.meter";

    public override string UnitItemId => Id;
    public override string Name => RS.MeterDistanceUnit_Name;
    public override string Description => RS.MeterDistanceUnit_Description;
    public override string Symbol => RS.MeterDistanceUnit_Symbol;
    public override bool IsInternationalSystemUnit => true;
}

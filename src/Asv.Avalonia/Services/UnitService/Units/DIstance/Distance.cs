using System.Composition;
using Material.Icons;

namespace Asv.Avalonia;

[ExportUnit]
[Shared]
[method: ImportingConstructor]
public class Distance([ImportMany(Distance.Id)] IEnumerable<IUnitItem> items) : UnitBase(items)
{
    public const string Id = "distance";

    public override MaterialIconKind Icon => MaterialIconKind.LocationDistance;
    public override string Name => "Distance"; // TODO: Localize
    public override string Description => "Distance units"; // TODO: Localize
    public override string UnitId => Id;
}

[ExportUnitItem(Distance.Id)]
[Shared]
[method: ImportingConstructor]
public class MeterDistanceUnit() : UnitItemBase(1.0)
{
    public const string Id = $"{Distance.Id}.meter";

    public override string UnitItemId => Id;
    public override string Name => "Meter"; // TODO: Localize
    public override string Description => "Distance in meter"; // TODO: Localize
    public override string Symbol => "m"; // TODO: Localize
    public override bool IsInternationalSystemUnit => true;
}

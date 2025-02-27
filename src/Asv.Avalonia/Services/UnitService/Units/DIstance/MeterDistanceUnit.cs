using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(DistanceBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class MeterDistanceUnit() : UnitItemBase(1.0)
{
    public const string Id = $"{DistanceBase.Id}.meter";

    public override string UnitItemId => Id;
    public override string Name => RS.Meter_UnitItem_Name;
    public override string Description => RS.Meter_Distance_Description;
    public override string Symbol => RS.Meter_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}

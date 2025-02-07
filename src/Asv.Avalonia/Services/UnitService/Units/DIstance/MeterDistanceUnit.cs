using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(DistanceBase.Id)]
[Shared]
[method: ImportingConstructor]
public class MeterDistanceUnit() : UnitItemBase(1.0)
{
    public const string Id = $"{DistanceBase.Id}.meter";

    public override string UnitItemId => Id;
    public override string Name => RS.MeterDistanceUnit_Name;
    public override string Description => RS.MeterDistanceUnit_Description;
    public override string Symbol => RS.MeterDistanceUnit_Symbol;
    public override bool IsInternationalSystemUnit => true;
}

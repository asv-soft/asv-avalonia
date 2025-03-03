using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(AltitudeBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class MeterAltitudeUnit() : UnitItemBase(1.0)
{
    public const string Id = $"{AltitudeBase.Id}.meter";

    public override string UnitItemId => Id;
    public override string Name => RS.Meter_UnitItem_Name;
    public override string Description => RS.Meter_Altitude_Description;
    public override string Symbol => RS.Meter_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}

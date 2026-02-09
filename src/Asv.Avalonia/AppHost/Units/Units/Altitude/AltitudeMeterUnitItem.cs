using System.Composition;

namespace Asv.Avalonia;

public sealed class AltitudeMeterUnitItem() : UnitItemBase(1.0)
{
    public const string Id = $"{AltitudeUnit.Id}.meter";

    public override string UnitItemId => Id;
    public override string Name => RS.Meter_UnitItem_Name;
    public override string Description => RS.Meter_Altitude_Description;
    public override string Symbol => RS.Meter_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}

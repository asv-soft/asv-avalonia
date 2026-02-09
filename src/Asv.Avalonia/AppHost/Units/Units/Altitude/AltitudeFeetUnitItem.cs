using System.Composition;

namespace Asv.Avalonia;

public sealed class AltitudeFeetUnitItem() : UnitItemBase(3.28)
{
    public const string Id = $"{AltitudeUnit.Id}.feet";

    public override string UnitItemId => Id;
    public override string Name => RS.Feet_UnitItem_Name;
    public override string Description => RS.Feet_Altitude_Description;
    public override string Symbol => RS.Feet_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}

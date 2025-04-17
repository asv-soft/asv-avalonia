using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(AltitudeBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class FeetAltitudeUnit() : UnitItemBase(3.28)
{
    public const string Id = $"{AltitudeBase.Id}.feet";

    public override string UnitItemId => Id;
    public override string Name => RS.Feet_UnitItem_Name;
    public override string Description => RS.Feet_Altitude_Description;
    public override string Symbol => RS.Feet_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}

using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(AltitudeBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class FeetAltitudeUnit() : UnitItemBase(0.3048)
{
    public const string Id = $"{AltitudeBase.Id}.feet";

    public override string UnitItemId => Id;
    public override string Name => RS.FeetAltitudeUnit_Name;
    public override string Description => RS.FeetAltitudeUnit_Description;
    public override string Symbol => RS.FeetAltitudeUnit_Symbol;
    public override bool IsInternationalSystemUnit => false;
}

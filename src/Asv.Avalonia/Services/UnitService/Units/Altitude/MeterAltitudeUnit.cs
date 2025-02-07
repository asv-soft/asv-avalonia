using System.Composition;

namespace Asv.Avalonia.Altitude;

[ExportUnitItem(AltitudeBase.Id)]
[Shared]
[method: ImportingConstructor]
public class MeterAltitudeUnit() : UnitItemBase(1.0)
{
    public const string Id = $"{AltitudeBase.Id}.meter";

    public override string UnitItemId => Id;
    public override string Name => RS.MeterAltitudeUnit_Name;
    public override string Description => RS.MeterAltitudeUnit_Description;
    public override string Symbol => RS.MeterAltitudeUnit_Symbol;
    public override bool IsInternationalSystemUnit => true;
}

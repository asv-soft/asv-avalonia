using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(VelocityBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class KilometersPerHourVelocityUnit() : UnitItemBase(3.6)
{
    public const string Id = $"{VelocityBase.Id}.kmh";

    public override string UnitItemId => Id;
    public override string Name => RS.KilometersPerHour_UnitItem_Name;
    public override string Description => RS.KilometersPerHour_Velocity_Description;
    public override string Symbol => RS.KilometersPerHour_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}

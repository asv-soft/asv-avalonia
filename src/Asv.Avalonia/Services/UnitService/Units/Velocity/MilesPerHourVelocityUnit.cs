using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(VelocityBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class MilesPerHourVelocityUnit() : UnitItemBase(2.236936)
{
    public const string Id = $"{VelocityBase.Id}.mih";

    public override string UnitItemId => Id;
    public override string Name => RS.MilesPerHour_UnitItem_Name;
    public override string Description => RS.MilesPerHour_Velocity_Description;
    public override string Symbol => RS.MilesPerHour_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}

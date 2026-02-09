using System.Composition;

namespace Asv.Avalonia;

public sealed class VelocityMilesPerHourUnitItem() : UnitItemBase(2.236936)
{
    public const string Id = $"{VelocityUnit.Id}.mih";

    public override string UnitItemId => Id;
    public override string Name => RS.MilesPerHour_UnitItem_Name;
    public override string Description => RS.MilesPerHour_Velocity_Description;
    public override string Symbol => RS.MilesPerHour_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}

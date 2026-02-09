using System.Composition;

namespace Asv.Avalonia;

public sealed class VelocityMetersPerSecondUnitItem() : UnitItemBase(1)
{
    public const string Id = $"{VelocityUnit.Id}.mps";

    public override string UnitItemId => Id;
    public override string Name => RS.MetersPerSecond_UnitItem_Name;
    public override string Description => RS.MetersPerSecond_Velocity_Description;
    public override string Symbol => RS.MetersPerSecond_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}

using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(VelocityBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class MetersPerSecondVelocityUnit() : UnitItemBase(1)
{
    public const string Id = $"{VelocityBase.Id}.mps";

    public override string UnitItemId => Id;
    public override string Name => RS.MetersPerSecond_UnitItem_Name;
    public override string Description => RS.MetersPerSecond_Velocity_Description;
    public override string Symbol => RS.MetersPerSecond_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}

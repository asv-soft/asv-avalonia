using System.Composition;

namespace Asv.Avalonia;

public sealed class FieldStrengthMicroVoltsPerMeterUnitItem() : UnitItemBase(1)
{
    public const string Id = $"{FieldStrengthUnit.Id}.mvpm";

    public override string UnitItemId => Id;
    public override string Name => RS.MicroVoltsPerMeter_UnitItem_Name;
    public override string Description => RS.MicroVoltsPerMeter_FieldStrength_Description;
    public override string Symbol => RS.FieldStrength_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}

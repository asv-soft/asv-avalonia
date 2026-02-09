using System.Composition;

namespace Asv.Avalonia;

public sealed class AmModulationInPartsUnitItem() : UnitItemBase(1)
{
    public const string Id = $"{AmModulationUnit.Id}.in.parts";

    public override string UnitItemId => Id;
    public override string Name => RS.InParts_UnitItem_Name;
    public override string Description => RS.InParts_AmplitudeModulation_Description;
    public override string Symbol => string.Empty;
    public override bool IsInternationalSystemUnit => true;
}

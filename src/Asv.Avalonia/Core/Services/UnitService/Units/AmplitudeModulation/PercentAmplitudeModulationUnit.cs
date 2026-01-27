using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(AmplitudeModulationBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class PercentAmplitudeModulationUnit() : UnitItemBase(100)
{
    public const string Id = $"{AmplitudeModulationBase.Id}.percent";

    public override string UnitItemId => Id;
    public override string Name => RS.Percent_UnitItem_Name;
    public override string Description => RS.Percent_AmplitudeModulation_Description;
    public override string Symbol => "%";
    public override bool IsInternationalSystemUnit => false;
}

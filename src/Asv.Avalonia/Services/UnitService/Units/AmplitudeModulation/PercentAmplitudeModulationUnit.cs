using System.Composition;

namespace Asv.Avalonia.AmplitudeModulation;

[ExportUnitItem(AmplitudeModulationBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class PercentAmplitudeModulationUnit() : UnitItemBase(0.01)
{
    public const string Id = $"{AmplitudeModulationBase.Id}.percent";

    public override string UnitItemId => Id;
    public override string Name => RS.PercentAmplitudeModulation_Name;
    public override string Description => RS.PercentAmplitudeModulation_Description;
    public override string Symbol => RS.PercentAmplitudeModulation_Symbol;
    public override bool IsInternationalSystemUnit => false;
}

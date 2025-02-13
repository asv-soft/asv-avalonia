using System.Composition;

namespace Asv.Avalonia.AmplitudeModulation;

[ExportUnitItem(AmplitudeModulationBase.Id)]
[Shared]
[method: ImportingConstructor]
public class InPartsAmplitudeModulationUnit() : UnitItemBase(1)
{
    public const string Id = $"{AmplitudeModulationBase.Id}.inparts";

    public override string UnitItemId => Id;
    public override string Name => RS.InPartsAmplitudeModulation_Name;
    public override string Description => RS.InPartsAmplitudeModulation_Description;
    public override string Symbol => RS.InPartsAmplitudeModulation_Symbol;
    public override bool IsInternationalSystemUnit => true;
}

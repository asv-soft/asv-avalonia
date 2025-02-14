using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(AmplitudeModulationBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class InPartsAmplitudeModulationUnit() : UnitItemBase(1)
{
    public const string Id = $"{AmplitudeModulationBase.Id}.inparts";

    public override string UnitItemId => Id;
    public override string Name => RS.InPartsAmplitudeModulation_Name;
    public override string Description => RS.InPartsAmplitudeModulation_Description;
    public override string Symbol => string.Empty;
    public override bool IsInternationalSystemUnit => true;
}

using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(AmperageBase.Id)]
[Shared]
[method: ImportingConstructor]

public sealed class AmperageAmpereUnit() : UnitItemBase(1)
{
    public const string Id = $"{AmplitudeModulationBase.Id}.amperes";

    public override string UnitItemId => Id;
    public override string Name => RS.AmperageAmpereUnit_Name;
    public override string Description => RS.AmperageAmpereUnit_Description;
    public override string Symbol => RS.AmperageAmpereUnit_Symbol;
    public override bool IsInternationalSystemUnit => true;
}
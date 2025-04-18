using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(AmperageBase.Id)]
[Shared]
[method: ImportingConstructor]

public sealed class AmperageMilliAmpereUnit() : UnitItemBase(0.001)
{
    public const string Id = $"{AmplitudeModulationBase.Id}.milliAmpere";
    public override string UnitItemId => Id;
    public override string Name => RS.AmperageMilliAmpereUnit_Name;
    public override string Description => RS.AmperageMilliAmpereUnit_Description;
    public override string Symbol => RS.AmperageMilliAmpereUnit_Symbol;
    public override bool IsInternationalSystemUnit => true;
}
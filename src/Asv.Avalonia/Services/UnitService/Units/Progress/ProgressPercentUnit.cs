using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(ProgressBase.Id)]
[Shared]
[method: ImportingConstructor]
public class ProgressPercentUnit() : UnitItemBase(100)
{
    public const string Id = $"{AmplitudeModulationBase.Id}.percent";
    public override string UnitItemId => Id;
    public override string Name => RS.ProgressPercentUnit_Name;
    public override string Description => RS.ProgressPercentUnit_Description;
    public override string Symbol => "%";
    public override bool IsInternationalSystemUnit => true;
}
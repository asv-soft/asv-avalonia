using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(AmperageBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class MilliAmpereAmperageUnit() : UnitItemBase(1000)
{
    public const string Id = $"{AmperageBase.Id}.milliamp";
    public override string UnitItemId => Id;
    public override string Name => RS.MilliAmpere_UnitItem_Name;
    public override string Description => RS.MilliAmpere_Amperage_Description;
    public override string Symbol => RS.MilliAmpere_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}

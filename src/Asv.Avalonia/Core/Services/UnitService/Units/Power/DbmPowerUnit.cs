using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(PowerBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class DbmPowerUnit() : UnitItemBase(1)
{
    public const string Id = $"{PowerBase.Id}.dbm";

    public override string UnitItemId => Id;
    public override string Name => RS.Dbm_UnitItem_Name;
    public override string Description => RS.Dbm_Power_Description;
    public override string Symbol => RS.Dbm_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}

using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(TimeSpanBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class SecondTimeSpanUnit() : UnitItemBase(1)
{
    public const string Id = $"{TimeSpanBase.Id}.second";

    public override string UnitItemId => Id;
    public override string Name => RS.Second_UnitItem_Name;
    public override string Description => RS.Second_TimeSpan_Description;
    public override string Symbol => RS.Second_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => true;
}

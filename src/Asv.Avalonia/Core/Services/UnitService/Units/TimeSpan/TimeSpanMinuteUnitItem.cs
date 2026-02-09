using System.Composition;

namespace Asv.Avalonia;

public sealed class TimeSpanMinuteUnitItem() : UnitItemBase(1 / 60.0)
{
    public const string Id = $"{TimeSpanUnit.Id}.minute";

    public override string UnitItemId => Id;
    public override string Name => RS.Minute_UnitItem_Name;
    public override string Description => RS.Minute_TimeSpan_Description;
    public override string Symbol => RS.Minute_UnitItem_Symbol;
    public override bool IsInternationalSystemUnit => false;
}

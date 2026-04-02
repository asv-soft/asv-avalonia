using Material.Icons;

namespace Asv.Avalonia;

public sealed class NullUnitBase(IEnumerable<IUnitItem> items) : UnitBase(items)
{
    public const string Id = "null.unit";

    protected override void SetUnitItem(IUnitItem unitItem)
    {
        return;
    }

    public override MaterialIconKind Icon => MaterialIconKind.Settings;
    public override string Name => "Null";
    public override string Description => "Null unit";
    public override string UnitId => Id;
}

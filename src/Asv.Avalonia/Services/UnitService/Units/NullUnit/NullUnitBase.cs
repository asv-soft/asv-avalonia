using Asv.Cfg;
using Material.Icons;

namespace Asv.Avalonia;

public class NullUnitBase(IEnumerable<IUnitItem> items)
    : UnitBase(new InMemoryConfiguration(), items)
{
    public const string Id = "null.unit";

    public override MaterialIconKind Icon => MaterialIconKind.Settings;
    public override string Name => "Null";
    public override string Description => "Null unit";
    public override string UnitId => Id;
}

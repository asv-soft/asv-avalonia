using System.Composition;
using Asv.Cfg;
using Material.Icons;

namespace Asv.Avalonia.Altitude;

[ExportUnit]
[Shared]
[method: ImportingConstructor]
public class AltitudeBase(
    [Import] IConfiguration cfgSvc,
    [ImportMany(AltitudeBase.Id)] IEnumerable<IUnitItem> items
) : UnitBase(cfgSvc, items)
{
    public const string Id = "altitude";

    public override MaterialIconKind Icon => MaterialIconKind.ArrowUpward;
    public override string Name => RS.Altitude_Name;
    public override string Description => RS.Altitude_Description;
    public override string UnitId => Id;
}

using System.Composition;
using Asv.Cfg;
using Material.Icons;

namespace Asv.Avalonia;

public class AltitudeConfig
{
    public string? CurrentUnitItemId { get; set; }
}

[ExportUnit]
[Shared]
public class AltitudeBase : UnitBase
{
    public const string Id = "altitude";

    public override MaterialIconKind Icon => MaterialIconKind.ArrowUpward;
    public override string Name => RS.Altitude_Name;
    public override string Description => RS.Altitude_Description;
    public override string UnitId => Id;

    private readonly AltitudeConfig _config;
    private readonly IConfiguration _cfg;

    [ImportingConstructor]
    public AltitudeBase(
        [Import] IConfiguration cfgSvc,
        [ImportMany(Id)] IEnumerable<IUnitItem> items
    )
        : base(items)
    {
        ArgumentNullException.ThrowIfNull(cfgSvc);
        _cfg = cfgSvc;
        _config = cfgSvc.Get<AltitudeConfig>();

        if (_config.CurrentUnitItemId is null)
        {
            return;
        }

        AvailableUnits.TryGetValue(_config.CurrentUnitItemId, out var unit);
        if (unit is not null)
        {
            Current.OnNext(unit);
        }
    }

    protected override void SetUnitItem(IUnitItem unitItem)
    {
        if (_config.CurrentUnitItemId == unitItem.UnitItemId)
        {
            return;
        }

        _config.CurrentUnitItemId = unitItem.UnitItemId;
        _cfg.Set(_config);
    }
}

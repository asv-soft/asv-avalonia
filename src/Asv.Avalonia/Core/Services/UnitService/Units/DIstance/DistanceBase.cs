using System.Composition;
using Asv.Cfg;
using Material.Icons;

namespace Asv.Avalonia;

internal sealed class DistanceConfig
{
    public string? CurrentUnitItemId { get; set; }
}

[ExportUnit]
[Shared]
public sealed class DistanceBase : UnitBase
{
    public const string Id = "distance";

    public override MaterialIconKind Icon => MaterialIconKind.LocationDistance;
    public override string Name => RS.Distance_Name;
    public override string Description => RS.Distance_Description;
    public override string UnitId => Id;

    private readonly DistanceConfig? _config;
    private readonly IConfiguration _cfgSvc;

    [ImportingConstructor]
    public DistanceBase(
        [Import] IConfiguration cfgSvc,
        [ImportMany(Id)] IEnumerable<IUnitItem> items
    )
        : base(items)
    {
        ArgumentNullException.ThrowIfNull(cfgSvc);
        _cfgSvc = cfgSvc;
        _config = cfgSvc.Get<DistanceConfig>();
        if (_config.CurrentUnitItemId is null)
        {
            return;
        }

        AvailableUnits.TryGetValue(_config.CurrentUnitItemId, out var unit);
        if (unit is not null)
        {
            CurrentUnitItem.OnNext(unit);
        }
    }

    protected override void SetUnitItem(IUnitItem unitItem)
    {
        if (_config is null)
        {
            return;
        }

        if (_config.CurrentUnitItemId == unitItem.UnitItemId)
        {
            return;
        }

        _config.CurrentUnitItemId = unitItem.UnitItemId;
        _cfgSvc.Set(_config);
    }
}

using System.Composition;
using Asv.Cfg;
using Material.Icons;

namespace Asv.Avalonia;

internal sealed class LatitudeConfig
{
    public string? CurrentUnitItemId { get; set; }
}

[ExportUnit]
[Shared]
public sealed class LatitudeBase : UnitBase
{
    public const string Id = "latitude";

    public override MaterialIconKind Icon => MaterialIconKind.Latitude;
    public override string Name => RS.Latitude_Name;
    public override string Description => RS.Latitude_Description;
    public override string UnitId => Id;

    private readonly LatitudeConfig? _config;
    private readonly IConfiguration _cfgSvc;

    [ImportingConstructor]
    public LatitudeBase(
        [Import] IConfiguration cfgSvc,
        [ImportMany(Id)] IEnumerable<IUnitItem> items
    )
        : base(items)
    {
        ArgumentNullException.ThrowIfNull(cfgSvc);
        _cfgSvc = cfgSvc;
        _config = cfgSvc.Get<LatitudeConfig>();
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

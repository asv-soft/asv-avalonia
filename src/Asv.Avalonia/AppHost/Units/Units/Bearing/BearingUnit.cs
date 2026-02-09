using System.Composition;
using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

internal sealed class BearingConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class BearingUnit : UnitBase
{
    public const string Id = "bearing";

    public override MaterialIconKind Icon => MaterialIconKind.Location;
    public override string Name => RS.Bearing_Name;
    public override string Description => RS.Bearing_Description;
    public override string UnitId => Id;

    private readonly BearingConfig? _config;
    private readonly IConfiguration _cfgSvc;

    public BearingUnit(IConfiguration cfgSvc, [FromKeyedServices(Id)] IEnumerable<IUnitItem> items)
        : base(items)
    {
        ArgumentNullException.ThrowIfNull(cfgSvc);
        _cfgSvc = cfgSvc;
        _config = cfgSvc.Get<BearingConfig>();
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

using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

internal sealed class ThrottleConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class ThrottleUnit : UnitBase
{
    public const string Id = "throttle";

    public override MaterialIconKind Icon => MaterialIconKind.Timelapse;
    public override string Name => RS.Throttle_Name;
    public override string Description => RS.Throttle_Description;
    public override string UnitId => Id;

    private readonly ThrottleConfig? _config;
    private readonly IConfiguration _cfgSvc;

    public ThrottleUnit(IConfiguration cfgSvc, [FromKeyedServices(Id)] IEnumerable<IUnitItem> items)
        : base(items)
    {
        ArgumentNullException.ThrowIfNull(cfgSvc);
        _cfgSvc = cfgSvc;
        _config = cfgSvc.Get<ThrottleConfig>();
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

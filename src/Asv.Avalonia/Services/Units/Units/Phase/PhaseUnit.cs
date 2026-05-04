using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

internal sealed class PhaseConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class PhaseUnit : UnitBase
{
    public const string Id = "phase";

    public override MaterialIconKind Icon => MaterialIconKind.PowerPlug;
    public override string Name => RS.Phase_Name;
    public override string Description => RS.Phase_Description;
    public override string UnitId => Id;

    private readonly PhaseConfig? _config;
    private readonly IConfiguration _cfgSvc;

    public PhaseUnit(IConfiguration cfgSvc, [FromKeyedServices(Id)] IEnumerable<IUnitItem> items)
        : base(items)
    {
        ArgumentNullException.ThrowIfNull(cfgSvc);
        _cfgSvc = cfgSvc;
        _config = cfgSvc.Get<PhaseConfig>();
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

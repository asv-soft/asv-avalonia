using System.Composition;
using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

internal sealed class DegreeConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class AngleUnit : UnitBase
{
    public const string Id = "angle";

    public override MaterialIconKind Icon => MaterialIconKind.AngleRight;
    public override string Name => RS.Angle_Name;
    public override string Description => RS.Angle_Description;
    public override string UnitId => Id;

    private readonly DegreeConfig? _config;
    private readonly IConfiguration _cfgSvc;

    public AngleUnit(IConfiguration cfgSvc, [FromKeyedServices(Id)] IEnumerable<IUnitItem> items)
        : base(items)
    {
        ArgumentNullException.ThrowIfNull(cfgSvc);
        _cfgSvc = cfgSvc;
        _config = cfgSvc.Get<DegreeConfig>();
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

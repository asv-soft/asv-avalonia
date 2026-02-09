using System.Composition;
using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

internal sealed class LongitudeConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class LongitudeUnit : UnitBase
{
    public const string Id = "longitude";

    public override MaterialIconKind Icon => MaterialIconKind.Longitude;
    public override string Name => RS.Longitude_Name;
    public override string Description => RS.Longitude_Description;
    public override string UnitId => Id;

    private readonly LongitudeConfig? _config;
    private readonly IConfiguration _cfgSvc;

    public LongitudeUnit(
        IConfiguration cfgSvc,
        [FromKeyedServices(Id)] IEnumerable<IUnitItem> items
    )
        : base(items)
    {
        ArgumentNullException.ThrowIfNull(cfgSvc);
        _cfgSvc = cfgSvc;
        _config = cfgSvc.Get<LongitudeConfig>();
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

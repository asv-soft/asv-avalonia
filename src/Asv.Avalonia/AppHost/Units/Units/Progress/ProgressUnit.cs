using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

internal sealed class ProgressConfig
{
    public string? CurrentUnitItemId { get; set; }
}

public sealed class ProgressUnit : UnitBase
{
    public const string Id = "progress";
    private readonly ProgressConfig? _config;
    private readonly IConfiguration _cfgSvc;

    public ProgressUnit(IConfiguration cfgSvc, [FromKeyedServices(Id)] IEnumerable<IUnitItem> items)
        : base(items)
    {
        ArgumentNullException.ThrowIfNull(cfgSvc);
        _cfgSvc = cfgSvc;
        _config = cfgSvc.Get<ProgressConfig>();

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

    public override MaterialIconKind Icon => MaterialIconKind.ProgressDownload;
    public override string Name => RS.Progress_Name;
    public override string Description => RS.Progress_Description;
    public override string UnitId => Id;
}

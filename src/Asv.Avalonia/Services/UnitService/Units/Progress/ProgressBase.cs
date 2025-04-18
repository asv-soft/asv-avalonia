using System.Composition;
using Asv.Cfg;
using Material.Icons;

namespace Asv.Avalonia;
internal sealed class ProgressConfig
{
    public string? CurrentUnitItemId { get; set; }
}

[ExportUnit]
[Shared]
public sealed class ProgressBase : UnitBase
{
    public const string Id = "progress";
    private readonly ProgressConfig? _config;
    private readonly IConfiguration _cfgSvc;
    
    public ProgressBase(
        [Import] IConfiguration cfgSvc,
        [ImportMany(Id)] IEnumerable<IUnitItem> items) : base(items)
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
            Current.OnNext(unit);
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

    public override MaterialIconKind Icon => MaterialIconKind.ProgressWrench;
    public override string Name => RS.ProgressBase_Name;
    public override string Description => RS.ProgressBase_Description;
    public override string UnitId => Id;
}
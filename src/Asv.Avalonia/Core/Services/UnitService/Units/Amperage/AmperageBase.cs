using System.Composition;
using Asv.Cfg;
using Material.Icons;

namespace Asv.Avalonia;

internal sealed class AmperageConfig
{
    public string? CurrentUnitItemId { get; set; }
}

[ExportUnit]
[Shared]
public class AmperageBase : UnitBase
{
    private readonly AmperageConfig? _config;
    private readonly IConfiguration _cfgSvc;
    public const string Id = "amperage";

    public override MaterialIconKind Icon => MaterialIconKind.Electricity;
    public override string Name => RS.Amperage_Name;
    public override string Description => RS.Amperage_Description;
    public override string UnitId => Id;

    [ImportingConstructor]
    public AmperageBase(
        [Import] IConfiguration cfgSvc,
        [ImportMany(Id)] IEnumerable<IUnitItem> items
    )
        : base(items)
    {
        ArgumentNullException.ThrowIfNull(cfgSvc);
        _cfgSvc = cfgSvc;
        _config = cfgSvc.Get<AmperageConfig>();
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

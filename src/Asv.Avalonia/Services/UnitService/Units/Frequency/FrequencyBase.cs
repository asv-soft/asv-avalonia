using System.Composition;
using Asv.Cfg;
using Material.Icons;

namespace Asv.Avalonia;

internal sealed class FrequencyConfig
{
    public string? CurrentUnitItemId { get; set; }
}

[ExportUnit]
[Shared]
public sealed class FrequencyBase : UnitBase
{
    public const string Id = "frequency";

    public override MaterialIconKind Icon => MaterialIconKind.Frequency;
    public override string Name => RS.Frequency_Name;
    public override string Description => RS.Frequency_Description;
    public override string UnitId => Id;

    private readonly FrequencyConfig? _config;
    private readonly IConfiguration _cfgSvc;

    [ImportingConstructor]
    public FrequencyBase(
        [Import] IConfiguration cfgSvc,
        [ImportMany(Id)] IEnumerable<IUnitItem> items
    )
        : base(items)
    {
        ArgumentNullException.ThrowIfNull(cfgSvc);
        _cfgSvc = cfgSvc;
        _config = cfgSvc.Get<FrequencyConfig>();
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
}

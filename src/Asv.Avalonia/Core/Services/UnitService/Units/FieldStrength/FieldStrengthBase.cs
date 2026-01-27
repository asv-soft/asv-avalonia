using System.Composition;
using Asv.Cfg;
using Material.Icons;

namespace Asv.Avalonia;

internal sealed class FieldStrengthConfig
{
    public string? CurrentUnitItemId { get; set; }
}

[ExportUnit]
[Shared]
public sealed class FieldStrengthBase : UnitBase
{
    public const string Id = "field.strength";

    public override MaterialIconKind Icon => MaterialIconKind.HighVoltage;
    public override string Name => RS.FieldStrength_Name;
    public override string Description => RS.FieldStrength_Description;
    public override string UnitId => Id;

    private readonly FieldStrengthConfig? _config;
    private readonly IConfiguration _cfgSvc;

    [ImportingConstructor]
    public FieldStrengthBase(
        [Import] IConfiguration cfgSvc,
        [ImportMany(Id)] IEnumerable<IUnitItem> items
    )
        : base(items)
    {
        ArgumentNullException.ThrowIfNull(cfgSvc);
        _cfgSvc = cfgSvc;
        _config = cfgSvc.Get<FieldStrengthConfig>();
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

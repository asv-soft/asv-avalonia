using System.Composition;
using Asv.Cfg;
using Material.Icons;

namespace Asv.Avalonia;

internal sealed class SdmConfig
{
    public string? CurrentUnitItemId { get; set; }
}

[ExportUnit]
[Shared]
public sealed class SdmBase : UnitBase
{
    public const string Id = "sdm";

    public override MaterialIconKind Icon => MaterialIconKind.Amplitude;
    public override string Name => RS.Sdm_Name;
    public override string Description => RS.Sdm_Description;
    public override string UnitId => Id;

    private readonly SdmConfig? _config;
    private readonly IConfiguration _cfgSvc;

    [ImportingConstructor]
    public SdmBase([Import] IConfiguration cfgSvc, [ImportMany(Id)] IEnumerable<IUnitItem> items)
        : base(items)
    {
        ArgumentNullException.ThrowIfNull(cfgSvc);
        _cfgSvc = cfgSvc;
        _config = cfgSvc.Get<SdmConfig>();
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

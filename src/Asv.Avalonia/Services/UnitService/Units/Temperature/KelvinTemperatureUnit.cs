using System.Composition;

namespace Asv.Avalonia;

[ExportUnitItem(TemperatureBase.Id)]
[Shared]
[method: ImportingConstructor]
public sealed class KelvinTemperatureUnit() : UnitItemBase(1)
{
    public const string Id = $"{TemperatureBase.Id}.kelvin";

    public override string UnitItemId => Id;
    public override string Name => RS.Kelvin_UnitItem_Name;
    public override string Description => RS.Kelvin_Temperature_Description;
    public override string Symbol => "K";
    public override bool IsInternationalSystemUnit => true;
}

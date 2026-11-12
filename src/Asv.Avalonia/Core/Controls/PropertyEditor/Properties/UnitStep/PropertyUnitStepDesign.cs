using Material.Icons;

namespace Asv.Avalonia;

public sealed class PropertyUnitStepDesign : PropertyUnitStepViewModel
{
    public PropertyUnitStepDesign()
        : base(
            DesignTime.Id.TypeId,
            DesignTime.UnitService.GetRequiredUnitOfType<DistanceUnit>(DistanceUnit.Id)
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        Header = "Movement step";
        ShortHeader = "Step";
        Icon = MaterialIconKind.ArrowAll;
        Text.Value = "10";
    }
}

namespace Asv.Avalonia;

public static class FieldStrengthRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterFieldStrength(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .AddUnit<FieldStrengthUnit>(FieldStrengthUnit.Id)
            .AddItem<FieldStrengthMicroVoltsPerMeterUnitItem>();
        return builder;
    }

    public static IUnitItem? FieldStrength(this IUnitService service) =>
        service[FieldStrengthUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? FieldStrengthUvPerM(this IUnitService service) =>
        service[FieldStrengthUnit.Id, FieldStrengthMicroVoltsPerMeterUnitItem.Id];
}

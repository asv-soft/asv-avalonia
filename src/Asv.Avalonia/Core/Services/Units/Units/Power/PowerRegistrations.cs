namespace Asv.Avalonia;

public static class PowerRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterPower(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder.RegisterUnit<PowerUnit>(PowerUnit.Id).RegisterItem<PowerDbmUnitItem>();
        return builder;
    }

    public static IUnitItem? Power(this IUnitService service) =>
        service[PowerUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? PowerDbm(this IUnitService service) =>
        service[PowerUnit.Id, PowerDbmUnitItem.Id];
}

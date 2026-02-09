namespace Asv.Avalonia;

public static class PowerMixin
{
    public static UnitsBuilder RegisterPower(this UnitsBuilder builder)
    {
        builder.AddUnit<PowerUnit>(PowerUnit.Id).AddItem<PowerDbmUnitItem>();
        return builder;
    }

    public static IUnitItem? Power(this IUnitService service) =>
        service[PowerUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? PowerDbm(this IUnitService service) =>
        service[PowerUnit.Id, PowerDbmUnitItem.Id];
}

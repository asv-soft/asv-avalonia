namespace Asv.Avalonia;

public static class VoltageMixin
{
    public static UnitsHostBuilderMixin.Builder RegisterVoltage(
        this UnitsHostBuilderMixin.Builder builder
    )
    {
        builder
            .AddUnit<VoltageUnit>(VoltageUnit.Id)
            .AddItem<VoltageMilliVoltUnitItem>()
            .AddItem<VoltageVoltUnitItem>();
        return builder;
    }

    public static IUnitItem? Voltage(this IUnitService service)
    {
        return service[VoltageUnit.Id]?.CurrentUnitItem.Value;
    }

    public static IUnitItem? VoltageMilliVolt(this IUnitService service)
    {
        return service[VoltageUnit.Id, VoltageMilliVoltUnitItem.Id];
    }

    public static IUnitItem? VoltageVolt(this IUnitService service)
    {
        return service[VoltageUnit.Id, VoltageVoltUnitItem.Id];
    }
}

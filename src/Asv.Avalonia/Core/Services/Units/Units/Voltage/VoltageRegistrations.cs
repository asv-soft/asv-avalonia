namespace Asv.Avalonia;

public static class VoltageRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterVoltage(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .RegisterUnit<VoltageUnit>(VoltageUnit.Id)
            .RegisterItem<VoltageMilliVoltUnitItem>()
            .RegisterItem<VoltageVoltUnitItem>();
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

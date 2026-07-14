namespace Asv.Avalonia;

public static class DataRateRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterDataRate(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .RegisterUnit<DataRateUnit>(DataRateUnit.Id)
            .RegisterItem<DataRateBytePerSecondUnitItem>()
            .RegisterItem<DataRateKilobytePerSecondUnitItem>()
            .RegisterItem<DataRateMegabytePerSecondUnitItem>()
            .RegisterItem<DataRateGigabytePerSecondUnitItem>()
            .RegisterItem<DataRateTerabytePerSecondUnitItem>();
        return builder;
    }

    public static IUnitItem? DataRate(this IUnitService service) =>
        service[DataRateUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? DataRateBytePerSecond(this IUnitService service) =>
        service[DataRateUnit.Id, DataRateBytePerSecondUnitItem.Id];

    public static IUnitItem? DataRateKilobytePerSecond(this IUnitService service) =>
        service[DataRateUnit.Id, DataRateKilobytePerSecondUnitItem.Id];

    public static IUnitItem? DataRateMegabytePerSecond(this IUnitService service) =>
        service[DataRateUnit.Id, DataRateMegabytePerSecondUnitItem.Id];

    public static IUnitItem? DataRateGigabytePerSecond(this IUnitService service) =>
        service[DataRateUnit.Id, DataRateGigabytePerSecondUnitItem.Id];

    public static IUnitItem? DataRateTerabytePerSecond(this IUnitService service) =>
        service[DataRateUnit.Id, DataRateTerabytePerSecondUnitItem.Id];
}

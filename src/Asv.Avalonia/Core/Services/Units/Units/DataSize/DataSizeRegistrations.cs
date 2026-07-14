namespace Asv.Avalonia;

public static class DataSizeRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterDataSize(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .RegisterUnit<DataSizeUnit>(DataSizeUnit.Id)
            .RegisterItem<DataSizeByteUnitItem>()
            .RegisterItem<DataSizeKilobyteUnitItem>()
            .RegisterItem<DataSizeMegabyteUnitItem>()
            .RegisterItem<DataSizeGigabyteUnitItem>()
            .RegisterItem<DataSizeTerabyteUnitItem>();
        return builder;
    }

    public static IUnitItem? DataSize(this IUnitService service) =>
        service[DataSizeUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? DataSizeByte(this IUnitService service) =>
        service[DataSizeUnit.Id, DataSizeByteUnitItem.Id];

    public static IUnitItem? DataSizeKilobyte(this IUnitService service) =>
        service[DataSizeUnit.Id, DataSizeKilobyteUnitItem.Id];

    public static IUnitItem? DataSizeMegabyte(this IUnitService service) =>
        service[DataSizeUnit.Id, DataSizeMegabyteUnitItem.Id];

    public static IUnitItem? DataSizeGigabyte(this IUnitService service) =>
        service[DataSizeUnit.Id, DataSizeGigabyteUnitItem.Id];

    public static IUnitItem? DataSizeTerabyte(this IUnitService service) =>
        service[DataSizeUnit.Id, DataSizeTerabyteUnitItem.Id];
}

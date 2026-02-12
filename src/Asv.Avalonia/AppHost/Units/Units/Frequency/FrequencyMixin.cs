namespace Asv.Avalonia;

public static class FrequencyMixin
{
    public static  UnitsHostBuilderMixin.Builder RegisterFrequency(this  UnitsHostBuilderMixin.Builder builder)
    {
        builder
            .AddUnit<FrequencyUnit>(FrequencyUnit.Id)
            .AddItem<FrequencyGigahertzUnitItem>()
            .AddItem<FrequencyMegahertzUnitItem>()
            .AddItem<FrequencyKilohertzUnitItem>()
            .AddItem<FrequencyHertzUnitItem>();
        return builder;
    }

    public static IUnitItem? Frequency(this IUnitService service) =>
        service[FrequencyUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? FrequencyGHz(this IUnitService service) =>
        service[FrequencyUnit.Id, FrequencyGigahertzUnitItem.Id];

    public static IUnitItem? FrequencyMHz(this IUnitService service) =>
        service[FrequencyUnit.Id, FrequencyMegahertzUnitItem.Id];

    public static IUnitItem? FrequencyKHz(this IUnitService service) =>
        service[FrequencyUnit.Id, FrequencyKilohertzUnitItem.Id];

    public static IUnitItem? FrequencyHz(this IUnitService service) =>
        service[FrequencyUnit.Id, FrequencyHertzUnitItem.Id];
}

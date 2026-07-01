namespace Asv.Avalonia;

public static class PhaseRegistrations
{
    public static UnitServiceRegistrations.Builder RegisterPhase(
        this UnitServiceRegistrations.Builder builder
    )
    {
        builder
            .AddUnit<PhaseUnit>(PhaseUnit.Id)
            .AddItem<PhaseDegreeUnitItem>()
            .AddItem<PhaseRadianUnitItem>();
        return builder;
    }

    public static IUnitItem? Phase(this IUnitService service) =>
        service[PhaseUnit.Id]?.CurrentUnitItem.Value;

    public static IUnitItem? PhaseDegree(this IUnitService service) =>
        service[PhaseUnit.Id, PhaseDegreeUnitItem.Id];

    public static IUnitItem? PhaseRadian(this IUnitService service) =>
        service[PhaseUnit.Id, PhaseRadianUnitItem.Id];
}

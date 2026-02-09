using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class UnitsMixin
{
    public static IHostApplicationBuilder UseUnitService(
        this IHostApplicationBuilder builder,
        Action<UnitsBuilder>? configure = null
    )
    {
        builder.Services.AddSingleton<IUnitService, UnitService>();
        var unitsBuilder = new UnitsBuilder(builder);
        if (configure == null)
        {
            unitsBuilder.RegisterDefault();
        }
        else
        {
            configure(unitsBuilder);
        }

        return builder;
    }
}

public sealed class UnitsBuilder
{
    private readonly IHostApplicationBuilder _builder;

    internal UnitsBuilder(IHostApplicationBuilder builder)
    {
        _builder = builder;
    }

    public UnitItemBuilder AddUnit<TUnit>(string unitId)
        where TUnit : class, IUnit
    {
        _builder.Services.AddSingleton<IUnit, TUnit>();
        return new UnitItemBuilder(_builder, unitId);
    }

    public void RegisterDefault()
    {
        this.RegisterAltitude()
            .RegisterAmModulation()
            .RegisterAmperage()
            .RegisterAngle()
            .RegisterBearing()
            .RegisterCapacity()
            .RegisterDdmGp()
            .RegisterDdmLlz()
            .RegisterDistance()
            .RegisterFieldStrength()
            .RegisterFrequency()
            .RegisterThrottle()
            .RegisterLatitude()
            .RegisterLongitude()
            .RegisterPhase()
            .RegisterProgress()
            .RegisterSdm()
            .RegisterTemperature()
            .RegisterTimeSpan()
            .RegisterVelocity()
            .RegisterVoltage()
            .RegisterPower();
    }
}

public class UnitItemBuilder
{
    private readonly IHostApplicationBuilder _builder;
    private readonly string _itemId;

    internal UnitItemBuilder(IHostApplicationBuilder builder, string itemId)
    {
        _builder = builder;
        _itemId = itemId;
    }

    public UnitItemBuilder AddItem<TUnitItem>()
        where TUnitItem : class, IUnitItem
    {
        _builder.Services.AddKeyedSingleton<IUnitItem, TUnitItem>(_itemId);
        return this;
    }
}

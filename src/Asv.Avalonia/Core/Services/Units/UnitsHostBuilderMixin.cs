using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class UnitsHostBuilderMixin
{
    public static IHostApplicationBuilder UseUnitService(
        this IHostApplicationBuilder builder,
        Action<Builder>? configure = null
    )
    {
        builder.Services.AddSingleton<IUnitService, UnitService>();
        var unitsBuilder = new Builder(builder);
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

    public sealed class Builder
    {
        private readonly IHostApplicationBuilder _builder;

        internal Builder(IHostApplicationBuilder builder)
        {
            _builder = builder;
        }

        public ItemBuilder AddUnit<TUnit>(string unitId)
            where TUnit : class, IUnit
        {
            _builder.Services.AddSingleton<IUnit, TUnit>();
            return new ItemBuilder(_builder, unitId);
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

    public class ItemBuilder
    {
        private readonly IHostApplicationBuilder _builder;
        private readonly string _itemId;

        internal ItemBuilder(IHostApplicationBuilder builder, string itemId)
        {
            _builder = builder;
            _itemId = itemId;
        }

        public ItemBuilder AddItem<TUnitItem>()
            where TUnitItem : class, IUnitItem
        {
            _builder.Services.AddKeyedSingleton<IUnitItem, TUnitItem>(_itemId);
            return this;
        }
    }
}

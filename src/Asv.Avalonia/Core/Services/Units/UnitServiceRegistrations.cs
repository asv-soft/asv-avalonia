using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class UnitServiceRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder Units => builder.Core.Services.Units;
    }

    extension(ServicesRegistrations.Builder builder)
    {
        public Builder Units => new(builder);

        public ServicesRegistrations.Builder RegisterUnitService(Action<Builder>? configure = null)
        {
            builder.AppBuilder.Services.AddSingleton<IUnitService, UnitService>();
            var unitsBuilder = new Builder(builder);
            if (configure is null)
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

    public sealed class Builder : IDependencyBuilder
    {
        private readonly ServicesRegistrations.Builder _builder;

        internal Builder(ServicesRegistrations.Builder builder)
        {
            _builder = builder;
        }

        public IHostApplicationBuilder AppBuilder => _builder.AppBuilder;

        public ItemBuilder AddUnit<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TUnit
        >(string unitId)
            where TUnit : class, IUnit
        {
            _builder.AppBuilder.Services.AddSingleton<IUnit, TUnit>();
            return new ItemBuilder(this, unitId);
        }

        public Builder RegisterDefault()
        {
            return this.RegisterAltitude()
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
        private readonly Builder _builder;
        private readonly string _itemId;

        internal ItemBuilder(Builder builder, string itemId)
        {
            _builder = builder;
            _itemId = itemId;
        }

        public ItemBuilder AddItem<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TUnitItem
        >()
            where TUnitItem : class, IUnitItem
        {
            _builder.AppBuilder.Services.AddKeyedSingleton<IUnitItem, TUnitItem>(_itemId);
            return this;
        }
    }
}

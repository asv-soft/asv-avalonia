using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class ExtensionsRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder Extensions => builder.Core.Services.Extensions;
    }

    extension(ServicesRegistrations.Builder builder)
    {
        public Builder Extensions => new(builder);

        public ServicesRegistrations.Builder RegisterExtensions()
        {
            if (builder.AppBuilder.IsDesignTimeEnvironment)
            {
                return builder.RegisterDesignTimeExtensions();
            }

            builder.AppBuilder.Services.AddSingleton<IExtensionService, ExtensionService>();
            return builder;
        }

        private ServicesRegistrations.Builder RegisterDesignTimeExtensions()
        {
            builder.AppBuilder.Services.AddSingleton(NullExtensionService.Instance);
            return builder;
        }
    }

    public class Builder(ServicesRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder Register<
            TContext,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TExtension
        >()
            where TExtension : class, IExtensionFor<TContext>
        {
            builder.AppBuilder.Services.AddTransient<IExtensionFor<TContext>, TExtension>();
            return this;
        }

        public Builder Register<
            TContext,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TExtension
        >(string key)
            where TExtension : class, IExtensionFor<TContext>
        {
            builder.AppBuilder.Services.AddKeyedTransient<IExtensionFor<TContext>, TExtension>(key);
            return this;
        }

        public Builder RegisterPolicy<
            TContext,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TPolicy
        >()
            where TPolicy : class, IExtensionPolicyFor<TContext>
        {
            builder.AppBuilder.Services.AddTransient<IExtensionPolicyFor<TContext>, TPolicy>();
            return this;
        }

        public Builder RegisterPolicy<
            TContext,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TPolicy
        >(string key)
            where TPolicy : class, IExtensionPolicyFor<TContext>
        {
            builder.AppBuilder.Services.AddKeyedTransient<IExtensionPolicyFor<TContext>, TPolicy>(
                key
            );
            return this;
        }
    }
}

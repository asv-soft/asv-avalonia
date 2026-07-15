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

    /// <summary>
    /// Registers extensions and extension policies.
    /// </summary>
    /// <param name="builder">The core service registration builder.</param>
    public class Builder(ServicesRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        /// <summary>
        /// Registers an extension for every owner exposing the specified context type.
        /// </summary>
        /// <typeparam name="TContext">The type of object to extend.</typeparam>
        /// <typeparam name="TExtension">The extension implementation type.</typeparam>
        /// <returns>This builder.</returns>
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

        /// <summary>
        /// Registers an extension for owners whose type identifier matches the specified key.
        /// </summary>
        /// <typeparam name="TContext">The type of object to extend.</typeparam>
        /// <typeparam name="TExtension">The extension implementation type.</typeparam>
        /// <param name="key">The owner type identifier used as the dependency injection key.</param>
        /// <returns>This builder.</returns>
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

        /// <summary>
        /// Registers an extension policy for every owner exposing the specified context type.
        /// </summary>
        /// <typeparam name="TContext">The type of object whose extensions are filtered.</typeparam>
        /// <typeparam name="TPolicy">The extension policy implementation type.</typeparam>
        /// <returns>This builder.</returns>
        public Builder RegisterPolicy<
            TContext,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TPolicy
        >()
            where TPolicy : class, IExtensionPolicyFor<TContext>
        {
            builder.AppBuilder.Services.AddTransient<IExtensionPolicyFor<TContext>, TPolicy>();
            return this;
        }

        /// <summary>
        /// Registers an extension policy for owners whose type identifier matches the specified key.
        /// </summary>
        /// <typeparam name="TContext">The type of object whose extensions are filtered.</typeparam>
        /// <typeparam name="TPolicy">The extension policy implementation type.</typeparam>
        /// <param name="key">The owner type identifier used as the dependency injection key.</param>
        /// <returns>This builder.</returns>
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

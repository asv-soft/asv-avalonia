using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class ViewLocatorRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder ViewLocator => builder.Core.Services.ViewLocator;
    }

    extension(ServicesRegistrations.Builder builder)
    {
        public Builder ViewLocator => new(builder);

        public ServicesRegistrations.Builder RegisterViewLocator()
        {
            builder.AppBuilder.Services.AddSingleton<ViewLocator>();
            return builder;
        }
    }

    public class Builder(ServicesRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder RegisterViewFor<
            TViewModel,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView
        >()
            where TView : Control
            where TViewModel : IViewModel
        {
            builder.AppBuilder.Services.AddKeyedTransient<Control, TView>(
                ViewLocator.GetServiceKeyForView<TViewModel>()
            );
            return this;
        }
    }
}

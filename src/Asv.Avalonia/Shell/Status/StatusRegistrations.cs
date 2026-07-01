using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class StatusRegistrations
{
    extension(IHostApplicationBuilder builder)
    {
        public Builder Status => builder.Shell.Status;
    }

    extension(ShellRegistrations.Builder builder)
    {
        public Builder Status => new(builder);

        public ShellRegistrations.Builder RegisterStatus(Action<Builder>? configure = null)
        {
            configure ??= b => b.RegisterDefault();
            configure(new Builder(builder));
            return builder;
        }
    }

    public class Builder(ShellRegistrations.Builder builder) : IDependencyBuilder
    {
        public IHostApplicationBuilder AppBuilder => builder.AppBuilder;

        public Builder Register<
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
                TStatusViewModel,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TView
        >()
            where TStatusViewModel : class, IStatusItem
            where TView : Control
        {
            // register status item
            builder.AppBuilder.Services.AddKeyedTransient<IStatusItem, TStatusViewModel>(
                DefaultStatusExtender.Contract
            );
            builder.AppBuilder.ViewLocator.RegisterViewFor<TStatusViewModel, TView>();
            return this;
        }

        public Builder RegisterDefault()
        {
            RegisterDefaultStatusExtender();
            RegisterNavigationStatus();
            return this;
        }

        public Builder RegisterDefaultStatusExtender()
        {
            builder.AppBuilder.Extensions.Register<IShell, DefaultStatusExtender>();
            return this;
        }

        public Builder RegisterNavigationStatus()
        {
            Register<NavigationStatusItemViewModel, NavigationStatusItemView>();
            return this;
        }
    }
}

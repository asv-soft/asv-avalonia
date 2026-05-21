using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class StatusMixin
{
    extension(ShellMixin.Builder builder)
    {
        public Builder Status => new(builder);
    }

    public class Builder(ShellMixin.Builder builder)
    {
        public Builder Register<TStatusViewModel, TView>()
            where TStatusViewModel : class, IStatusItem
            where TView : Control
        {
            // register status item
            builder.Parent.Services.AddKeyedTransient<IStatusItem, TStatusViewModel>(
                DefaultStatusExtender.Contract
            );
            builder.Parent.ViewLocator.RegisterViewFor<TStatusViewModel, TView>();
            return this;
        }

        public Builder UseDefault()
        {
            builder.Parent.Extensions.Register<IShell, DefaultStatusExtender>();
            return this;
        }

        public Builder UseNavigationStatus()
        {
            Register<NavigationStatusItemViewModel, NavigationStatusItemView>();
            return this;
        }
    }
}

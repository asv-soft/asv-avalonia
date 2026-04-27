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
            builder.Parent.Services.AddTransient<IStatusItem, TStatusViewModel>();
            builder.Parent.ViewLocator.RegisterViewFor<TStatusViewModel, TView>();
            return this;
        }

        public Builder UseNavigationStatus()
        {
            Register<NavigationStatusItemViewModel, NavigationStatusItemView>();
            return this;
        }
    }
}
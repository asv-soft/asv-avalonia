using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class ViewLocatorMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseViewLocator()
        {
            builder.Services.AddSingleton<ViewLocator>();
            return builder;
        }

        public Builder ViewLocator => new(builder);
    }

    public class Builder(IHostApplicationBuilder builder)
    {
        public Builder RegisterViewFor<TViewModel, TView>()
            where TView : Control
            where TViewModel : IViewModel
        {
            builder.Services.AddKeyedTransient<Control, TView>(
                ViewLocator.GetServiceKeyForView<TViewModel>()
            );
            return this;
        }
    }
}

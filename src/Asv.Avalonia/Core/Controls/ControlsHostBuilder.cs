using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;



public static class ControlsHostBuilder
{
    public static IHostApplicationBuilder UseControls(
        this IHostApplicationBuilder builder,
        Action<Builder>? configure = null
    )
    {
        var controlsHostBuilder = new Builder(builder);
        if (configure == null)
        {
            controlsHostBuilder.RegisterDefault();
        }
        else
        {
            configure(controlsHostBuilder);
        }
        return builder;
    }
    
    public class Builder(IHostApplicationBuilder builder)
    {
        public void RegisterDefault()
        {
            this.RegisterWorkspace()
                .RegisterTreePage()
                .RttBox();
        }

        public Builder RegisterViewFor<TViewModel, TView>()
            where TView : Control
            where TViewModel : IViewModel
        {
            builder.Services.AddKeyedTransient<Control>(
                CompositionViewLocator.GetServiceKeyForView(typeof(TViewModel))
            );
            return this;
        }
    }
}



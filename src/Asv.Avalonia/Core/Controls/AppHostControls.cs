using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class AppHostControls
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
        public Builder RegisterDefault()
        {
            return this.RegisterWorkspace().RegisterTreePage().RegisterRttBox();
        }

        public Builder RegisterViewFor<TViewModel, TView>()
            where TView : Control
            where TViewModel : IViewModel
        {
            builder.Services.AddKeyedTransient<Control, TView>(
                CompositionViewLocator.GetServiceKeyForView(typeof(TViewModel))
            );
            return this;
        }
    }
}

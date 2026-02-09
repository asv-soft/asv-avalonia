using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class ControlsMixin
{
    public static string GetKeyForType(Type type) =>
        type.FullName ?? throw new ArgumentException("type is null", nameof(type));

    public static string GetKeyForType<T>() =>
        typeof(T).FullName ?? throw new ArgumentException("type is null", typeof(T).Name);

    public static IHostApplicationBuilder UseControls(
        this IHostApplicationBuilder builder,
        Action<ControlsHostBuilder>? configure = null
    )
    {
        var controlsHostBuilder = new ControlsHostBuilder(builder);
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
}

public class ControlsHostBuilder(IHostApplicationBuilder builder)
{
    public void RegisterDefault()
    {
        this.RegisterWorkspace();
    }

    public ControlsHostBuilder Register<TViewModel, TView>()
        where TView : Control
        where TViewModel : IViewModel
    {
        builder.Services.AddKeyedSingleton<Control, TView>(
            ControlsMixin.GetKeyForType(typeof(TViewModel))
        );
        return this;
    }
}

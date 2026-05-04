using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class ControlsMixin
{
    public static IHostApplicationBuilder UseDefaultControls(this IHostApplicationBuilder builder)
    {
        builder
            .ViewLocator.RegisterWorkspace()
            .RegisterTreePage()
            .RegisterRttBox()
            .RegisterPropertyEditor();
        return builder;
    }
}

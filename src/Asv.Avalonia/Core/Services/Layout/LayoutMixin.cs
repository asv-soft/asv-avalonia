using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class LayoutMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseLayoutService()
        {
            builder.Services.AddSingleton<ILayoutService, LayoutService>();
            return builder;
        }

        public IHostApplicationBuilder UseDesignTimeLayoutService()
        {
            builder.Services.AddSingleton<ILayoutService>(NullLayoutService.Instance);
            return builder;
        }
    }
}

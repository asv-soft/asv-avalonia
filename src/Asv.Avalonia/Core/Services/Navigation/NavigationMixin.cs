using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class NavigationMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseNavigationService()
        {
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            return builder;
        }

        public IHostApplicationBuilder UseDesignTimeNavigationService()
        {
            builder.Services.AddSingleton(NullNavigationService.Instance);
            return builder;
        }
    }
}

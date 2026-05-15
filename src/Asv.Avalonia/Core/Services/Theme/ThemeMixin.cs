using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class ThemeMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseThemeService()
        {
            builder.Services.AddSingleton<IThemeService, ThemeService>();
            return builder;
        }

        public IHostApplicationBuilder UseDesingTimeThemeService()
        {
            builder.Services.AddSingleton(NullThemeService.Instance);
            return builder;
        }
    }
}

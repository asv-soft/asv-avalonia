using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class ThemeRegistrations
{
    extension(ServicesRegistrations.Builder builder)
    {
        public ServicesRegistrations.Builder RegisterThemeService()
        {
            if (builder.AppBuilder.IsDesignTimeEnvironment)
            {
                return builder.RegisterDesignTimeThemeService();
            }

            builder.AppBuilder.Services.AddSingleton<IThemeService, ThemeService>();
            return builder;
        }

        private ServicesRegistrations.Builder RegisterDesignTimeThemeService()
        {
            builder.AppBuilder.Services.AddSingleton(NullThemeService.Instance);
            return builder;
        }
    }
}

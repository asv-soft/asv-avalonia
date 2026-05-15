using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class LocalizationMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseLocalizationService()
        {
            builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
            return builder;
        }

        public IHostApplicationBuilder UseDesignTimeLocalizationService()
        {
            builder.Services.AddSingleton<ILocalizationService>(NullLocalizationService.Instance);
            return builder;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class LocalizationRegistrations
{
    extension(ServicesRegistrations.Builder builder)
    {
        public ServicesRegistrations.Builder RegisterLocalizationService()
        {
            if (builder.AppBuilder.IsDesignTimeEnvironment)
            {
                return builder.RegisterDesignTimeLocalizationService();
            }

            builder.AppBuilder.Services.AddSingleton<ILocalizationService, LocalizationService>();
            return builder;
        }

        private ServicesRegistrations.Builder RegisterDesignTimeLocalizationService()
        {
            builder.AppBuilder.Services.AddSingleton(NullLocalizationService.Instance);
            return builder;
        }
    }
}

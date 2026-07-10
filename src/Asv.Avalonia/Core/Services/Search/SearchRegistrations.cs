using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class SearchRegistrations
{
    extension(ServicesRegistrations.Builder builder)
    {
        public ServicesRegistrations.Builder RegisterSearchService()
        {
            if (builder.AppBuilder.IsDesignTimeEnvironment)
            {
                return builder.RegisterDesignTimeSearchService();
            }

            builder.AppBuilder.Services.AddSingleton<ISearchService, SearchService>();
            return builder;
        }

        private ServicesRegistrations.Builder RegisterDesignTimeSearchService()
        {
            builder.AppBuilder.Services.AddSingleton<ISearchService>(NullSearchService.Instance);
            return builder;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class SearchMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseSearchService()
        {
            builder.Services.AddSingleton<ISearchService, SearchService>();
            return builder;
        }

        public IHostApplicationBuilder UseDesignTimeSearchService()
        {
            builder.Services.AddSingleton<ISearchService>(NullSearchService.Instance);
            return builder;
        }
    }
}

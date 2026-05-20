using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class AppArgsStoreMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseAppArgsStore()
        {
            builder.Services.AddSingleton<IAppArgsStore, AppArgsStore>();
            return builder;
        }

        public IHostApplicationBuilder UseDesignTimeAppArgsStore()
        {
            builder.Services.ReplaceSingleton(NullAppArgsStore.Instance);
            return builder;
        }
    }
}

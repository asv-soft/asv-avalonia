using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class UndoStoreMixin
{
    extension(IHostApplicationBuilder builder)
    {
        public IHostApplicationBuilder UseUndoStore(Action<UndoStoreServiceOptions>? configure = null)
        {
            configure ??= _ => { };
            builder.Services.AddSingleton<IUndoStoreService, UndoStoreService>()
                .AddOptions<UndoStoreServiceOptions>()
                .BindConfiguration(UndoStoreServiceOptions.SectionName)
                .PostConfigure(configure);
            return builder;
        }
    }
}
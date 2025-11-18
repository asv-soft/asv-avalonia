using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class TimeProviderMixin
{
    public static IHostApplicationBuilder UseTimeProvider(
        this IHostApplicationBuilder builder,
        TimeProvider? timeProvider = null
    )
    {
        builder.Services.AddSingleton(timeProvider ?? TimeProvider.System);
        return builder;
    }
}

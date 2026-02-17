using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia;

public static class ExtensionsMixin
{
    public static IHostApplicationBuilder UseExtensions(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IExtensionService, ExtensionService>();
        return builder;
    }

    public static IHostApplicationBuilder UseNullExtension(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton(NullExtensionService.Instance);
        return builder;
    }
}

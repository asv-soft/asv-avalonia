using Microsoft.Extensions.DependencyInjection;

namespace Asv.Avalonia;

public static class AppInfoMixin
{
    public static AsvHostBuilder UseAppInfo(this AsvHostBuilder builder)
    {
        builder.Services.AddSingleton<IAppInfo>(
            new AppInfo
            {
                Title = string.Empty,
                Name = string.Empty,
                Version = string.Empty,
                CompanyName = string.Empty,
                AvaloniaVersion = string.Empty,
            }
        );
        return builder;
    }
}

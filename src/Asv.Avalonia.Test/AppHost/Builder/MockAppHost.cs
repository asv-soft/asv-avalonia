using Microsoft.Extensions.Hosting;

namespace Asv.Avalonia.Test;

internal static class MockAppHost
{
    public static IHost Build(Action<IHostApplicationBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var contentRoot = Path.Combine(
            AppContext.BaseDirectory,
            "composition-tests",
            Guid.NewGuid().ToString("N")
        );
        Directory.CreateDirectory(contentRoot);

        var builder = Host.CreateApplicationBuilder(
            new HostApplicationBuilderSettings
            {
                Args = [],
                ApplicationName = typeof(MockAppHost).Assembly.GetName().Name,
                ContentRootPath = contentRoot,
                EnvironmentName = "Test",
            }
        );

        configure(builder);
        return builder.Build();
    }
}

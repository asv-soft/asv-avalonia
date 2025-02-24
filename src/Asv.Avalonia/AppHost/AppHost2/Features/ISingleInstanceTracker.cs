using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

Microsoft.Extensions.Configuration;

namespace Asv.Avalonia;

public interface ISingleInstanceTracker
{
    bool IsFirstInstance { get; }
}

public class SingleInstanceTrackerConfig
{
    public string? MutexName { get; set; }
}

public class SingleInstanceTracker : ISingleInstanceTracker
{
    public SingleInstanceTracker(IOptions<SingleInstanceTrackerConfig> option) { }

    public bool IsFirstInstance { get; }
}

public static class SingleInstanceTrackerMixin
{
    public static AsvHostBuilder UseSingleInstanceTracker(this AsvHostBuilder builder)
    {
        builder.Services.AddSingleton<ISingleInstanceTracker, SingleInstanceTracker>();
        builder
            .Services.AddOptions<SingleInstanceTrackerConfig>()
            .PostConfigure<IAppInfo>(
                (config, info) =>
                {
                    config.MutexName ??= info.Name;
                    config.PipeName ??= info.Name;
                }
            );

        return builder;
    }

    public static ISingleInstanceTracker GetSingleInstance(this AsvHost host)
    {
        return host.Services.GetRequiredService<ISingleInstanceTracker>();
    }

    public static void ExitIfNotFirstInstance(this AsvHost host)
    {
        if (host.GetSingleInstance().IsFirstInstance == false)
        {
            Environment.Exit(0);
        }
    }
}

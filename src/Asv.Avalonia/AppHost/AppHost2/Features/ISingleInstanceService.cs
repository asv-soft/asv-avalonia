using Asv.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Asv.Avalonia;

public interface ISingleInstanceService : IDisposable
{
    bool IsFirstInstance { get; }
}

public class SingleInstanceServiceConfig
{
    public const string Key = "Sis";
    public string? Mutex { get; set; }
    public bool ArgForward { get; set; }
    public string? Pipe { get; set; }
}

public class SingleInstanceService : AsyncDisposableOnce, ISingleInstanceService
{
    private readonly Mutex _mutex;
    public SingleInstanceService(IOptions<SingleInstanceServiceConfig> option)
    {
        var config = option.Value;
        _mutex = new Mutex(true, config.Mutex, out var isNewInstance);
        IsFirstInstance = isNewInstance;
    }

    public bool IsFirstInstance { get; }
}

public class SingleInstanceServiceBuilder(OptionsBuilder<SingleInstanceServiceConfig> options)
{
    public SingleInstanceServiceBuilder WithMutexName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        options.Configure(cfg => cfg.Mutex = name);
        return this;
    }
    
    public SingleInstanceServiceBuilder WithArgumentForwarding()
    {
        options.Configure(cfg => cfg.ArgForward = true);
        return this;
    }
    
    public SingleInstanceServiceBuilder WithArgumentForwarding(string pipeName)
    {
        ArgumentNullException.ThrowIfNull(pipeName);
        options.Configure(cfg =>
        {
            cfg.Pipe = pipeName;
            cfg.ArgForward = true;
        });
        return this;
    }
}

public static class SingleInstanceTrackerMixin
{
    public static AsvHostBuilder UseSingleInstance(this AsvHostBuilder builder, Action<SingleInstanceServiceBuilder>? configure = null)
    {
        var options = builder.Services
            .AddSingleton<ISingleInstanceService, SingleInstanceService>()
            .AddOptions<SingleInstanceServiceConfig>()
            .Bind(builder.Configuration.GetSection(SingleInstanceServiceConfig.Key))
            .PostConfigure<IAppInfo>((config, info) =>
                {
                    config.Mutex ??= info.Name;
                    config.Pipe ??= info.Name;
                }
            );
        var subBuilder = new SingleInstanceServiceBuilder(options);
        configure?.Invoke(subBuilder);
        return builder;
    }

    public static ISingleInstanceService GetSingleInstance(this AsvHost host)
    {
        return host.Services.GetRequiredService<ISingleInstanceService>();
    }

    public static void ExitIfNotFirstInstance(this AsvHost host)
    {
        if (host.GetSingleInstance().IsFirstInstance == false)
        {
            Environment.Exit(0);
        }
    }
}

using System.Globalization;
using Asv.Cfg;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia.Map;

internal static class MapCore
{
    public static ILoggerFactory LoggerFactory { get; } = NullLoggerFactory.Instance;
    public static ITileCache FastCache { get; } 
}

public class MapBuilder
{
    private Func<IConfiguration, ILoggerFactory, ITileCache> _fastCache;
    private Func<IConfiguration, ILoggerFactory, ITileCache> _slowCache;
    private Func<AppBuilder,IConfiguration> _config;
    private Func<AppBuilder, ILoggerFactory> _logger;

    protected internal MapBuilder(AppBuilder appBuilder)
    {
        _config = builder => builder.Instance?.TryGetFeature<IConfiguration>() ?? new InMemoryConfiguration();
        _logger = builder => builder.Instance?.TryGetFeature<ILoggerFactory>() ?? NullLoggerFactory.Instance;
        _fastCache = (cfg,log) => new MemoryTileCache(cfg.Get<MemoryTileCacheConfig>(), log);
        _slowCache = (cfg,log) => new FileSystemCache(cfg.Get<FileSystemCacheConfig>(), log);
        
    }

    public MapBuilder WithProvider(Func<IConfiguration, ILoggerFactory, ITileProvider> provider)
    {
        
    }
    public MapBuilder WithConfiguration(Func<AppBuilder, IConfiguration> config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _config = config;
        return this;
    }

    public MapBuilder WithLog(Func<AppBuilder, LoggerFactory> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _logger = factory;
        return this;
    }
    public MapBuilder WithFastCache(Func<IConfiguration, ILoggerFactory, ITileCache> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _fastCache = factory;
        return this;
    }

    public MapBuilder WithSlowCache(Func<IConfiguration, ILoggerFactory, ITileCache> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _slowCache = factory;
        return this;
    }
}

public static class MapBuilderMixin
{
    public static AppBuilder UseAsvMap(this AppBuilder builder, Action<MapBuilder> options)
    {
        return builder.AfterSetup(appBuilder =>
        {
            ServiceCollection coll = new ServiceCollection()
            var builder = new MapBuilder(appBuilder);
            options(builder);
            MapCore.Instance = builder.Build();
        });
    }
}
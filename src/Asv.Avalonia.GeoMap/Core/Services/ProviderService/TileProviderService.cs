using Asv.Cfg;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public class TileProviderServiceConfig
{
    public string CurrentProviderId { get; set; } = BingTileProvider.Id;

    public override string ToString()
    {
        return $"Current provider ID: {CurrentProviderId}";
    }
}

public class TileProviderService : ITileProviderService, IDisposable
{
    private readonly Lock _syncCfg = new();

    public TileProviderService(
        IEnumerable<ITileProvider> providers,
        IConfiguration configProvider,
        ILogger<TileProviderService> logger
    )
    {
        var config = configProvider.Get<TileProviderServiceConfig>();
        var providerList = providers.ToList();

        AvailableProviders = providerList.AsReadOnly();

        var currentProvider = providerList.FirstOrDefault(p =>
            p.Info.Id == config.CurrentProviderId
        );

        if (currentProvider == null)
        {
            logger.LogWarning(
                "Current tile provider '{ProviderId}' not found. Falling back to first available provider",
                config.CurrentProviderId
            );
            currentProvider = providerList.FirstOrDefault() ?? EmptyTileProvider.Instance;
        }

        CurrentProvider = new SynchronizedReactiveProperty<ITileProvider>(currentProvider);

        _configSyncSubscription = CurrentProvider
            .Skip(1)
            .Synchronize()
            .Subscribe(provider =>
            {
                using (_syncCfg.EnterScope())
                {
                    config.CurrentProviderId = provider.Info.Id;
                    configProvider.Set(config);
                }
            });
    }

    public IReadOnlyList<ITileProvider> AvailableProviders { get; }
    public SynchronizedReactiveProperty<ITileProvider> CurrentProvider { get; }

    #region Disposable

    private readonly IDisposable _configSyncSubscription;

    public void Dispose()
    {
        _configSyncSubscription.Dispose();
        CurrentProvider.Dispose();
    }

    #endregion
}

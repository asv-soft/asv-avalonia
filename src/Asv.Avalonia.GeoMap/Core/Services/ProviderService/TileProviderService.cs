using Asv.Cfg;
using Microsoft.Extensions.Logging;
using R3;

namespace Asv.Avalonia.GeoMap;

public class TileProviderServiceConfig
{
    public string CurrentProviderId { get; set; } = YandexTileProvider.Id;
    public Dictionary<string, string> ApiKeys { get; set; } = new();

    public override string ToString()
    {
        return $"Current provider ID: {CurrentProviderId}";
    }
}

public class TileProviderService : ITileProviderService, IDisposable
{
    private readonly Lock _syncCfg = new();
    private readonly IConfiguration _configProvider;
    private readonly TileProviderServiceConfig _config;

    public TileProviderService(
        IEnumerable<ITileProvider> providers,
        IConfiguration configProvider,
        ILogger<TileProviderService> logger
    )
    {
        _configProvider = configProvider;
        _config = configProvider.Get<TileProviderServiceConfig>();
        var providerList = providers.ToList();

        AvailableProviders = providerList.AsReadOnly();

        foreach (var provider in providerList)
        {
            if (
                provider is IProtectedTileProvider apiKeyProvider
                && _config.ApiKeys.TryGetValue(provider.Info.Id, out var savedKey)
            )
            {
                apiKeyProvider.ApiKey = savedKey;
            }
        }

        var currentProvider = providerList.FirstOrDefault(p =>
            p.Info.Id == _config.CurrentProviderId
        );

        if (currentProvider == null)
        {
            logger.LogWarning(
                "Current tile provider '{ProviderId}' not found. Falling back to first available provider",
                _config.CurrentProviderId
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
                    _config.CurrentProviderId = provider.Info.Id;
                    _configProvider.Set(_config);
                }
            });
    }

    public IReadOnlyList<ITileProvider> AvailableProviders { get; }
    public SynchronizedReactiveProperty<ITileProvider> CurrentProvider { get; }

    public void SetProviderApiKey(string providerId, string? apiKey)
    {
        var provider = AvailableProviders.FirstOrDefault(p => p.Info.Id == providerId);
        if (provider is not IProtectedTileProvider apiKeyProvider)
        {
            return;
        }

        apiKeyProvider.ApiKey = apiKey;

        using (_syncCfg.EnterScope())
        {
            if (apiKey != null)
            {
                _config.ApiKeys[providerId] = apiKey;
            }
            else
            {
                _config.ApiKeys.Remove(providerId);
            }

            _configProvider.Set(_config);
        }
    }

    public string? GetProviderApiKey(string providerId)
    {
        var provider = AvailableProviders.FirstOrDefault(p => p.Info.Id == providerId);
        if (provider is IProtectedTileProvider apiKeyProvider)
        {
            return apiKeyProvider.ApiKey;
        }

        return null;
    }

    #region Disposable

    private readonly IDisposable _configSyncSubscription;

    public void Dispose()
    {
        _configSyncSubscription.Dispose();
        CurrentProvider.Dispose();
    }

    #endregion
}

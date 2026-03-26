using R3;

namespace Asv.Avalonia.GeoMap;

public interface ITileProviderService
{
    IReadOnlyList<ITileProvider> AvailableProviders { get; }
    SynchronizedReactiveProperty<ITileProvider> CurrentProvider { get; }
    void SetProviderApiKey(string providerId, string? apiKey);
    string? GetProviderApiKey(string providerId);
}

using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public abstract class ProtectedHttpTileProvider(
    IHttpClientFactory httpClientFactory,
    ILogger logger,
    TimeProvider timeProvider
) : HttpTileProvider(httpClientFactory, logger, timeProvider), IProtectedTileProvider
{
    public string? ApiKey
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            ResetErrorState();
        }
    }
}

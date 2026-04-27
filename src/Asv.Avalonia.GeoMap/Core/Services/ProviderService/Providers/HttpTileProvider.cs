using System.Net;
using Asv.Common;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia.GeoMap;

public class HttpTileProviderConfig
{
    public int RequestTimeoutMs { get; set; } = 7000;

    public override string ToString()
    {
        return $"Timeout: {RequestTimeoutMs} ms";
    }
}

public abstract class HttpTileProvider : AsyncDisposableOnce, ITileProvider
{
    public const string HttpClientName = nameof(HttpTileProvider);

    private const int DefaultFailureThreshold = 3;
    private const int UnauthorizedFailureThreshold = 1;
    private static readonly TimeSpan InitialCooldown = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan MaxCooldown = TimeSpan.FromMinutes(3);

    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly TimeProvider _timeProvider;
    private readonly Lock _stateLock = new();

    private int _consecutiveFailures;
    private int _cooldownExponent;
    private bool _suspended;
    private bool _suspendedIndefinitely;
    private DateTimeOffset? _suspendedUntil;

    protected HttpTileProvider(
        IHttpClientFactory httpClientFactory,
        ILogger logger,
        TimeProvider timeProvider
    )
    {
        _httpClient = httpClientFactory.CreateClient(HttpClientName);
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public abstract TileProviderInfo Info { get; }
    public abstract IMapProjection Projection { get; }
    public virtual int TileSize => 256;

    protected abstract string GetTileUrl(TileKey key);

    public async Task<Tile?> DownloadAsync(TileKey key, CancellationToken ct)
    {
        if (IsSuspended())
        {
            return null;
        }

        var url = GetTileUrl(key);

        try
        {
            using var response = await _httpClient
                .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var contentLength = response.Content.Headers.ContentLength ?? 0;
            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            if (contentLength == 0)
            {
                await response.Content.LoadIntoBufferAsync(ct);
                contentLength = stream.Length;
            }

            var tile = Tile.Create(key, stream, (int)contentLength);

            HandleSuccess();

            return tile;
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("Tile download timed out for provider {ProviderId}", Info.Id);
            throw new DownloadTileException(
                ex.Message,
                RS.HttpTileProvider_Error_Timeout_Title,
                string.Format(RS.HttpTileProvider_Error_Timeout_Message, Info.Name)
            );
        }
        catch (Exception ex)
        {
            HandleFailure(ex);
        }

        return null;
    }

    protected void ResetErrorState()
    {
        HandleSuccess();
    }

    private bool IsSuspended()
    {
        using (_stateLock.EnterScope())
        {
            if (!_suspended)
            {
                return false;
            }

            if (_suspendedIndefinitely)
            {
                return true;
            }

            if (_suspendedUntil.HasValue && _timeProvider.GetUtcNow() >= _suspendedUntil.Value)
            {
                _suspended = false;
                _suspendedUntil = null;
                return false;
            }

            return true;
        }
    }

    private void HandleSuccess()
    {
        using (_stateLock.EnterScope())
        {
            _consecutiveFailures = 0;
            _cooldownExponent = 0;
            _suspended = false;
            _suspendedIndefinitely = false;
            _suspendedUntil = null;
        }
    }

    private void HandleFailure(Exception exception)
    {
        if (exception is OperationCanceledException)
        {
            throw exception;
        }

        using (_stateLock.EnterScope())
        {
            if (_suspended)
            {
                return;
            }

            _consecutiveFailures++;

            switch (exception)
            {
                case HttpRequestException
                {
                    StatusCode: HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                } ex:
                {
                    _logger.LogWarning(
                        "Authorization error in the tile provider {ProviderId} with status code {StatusCode}",
                        Info.Id,
                        ex.StatusCode
                    );

                    if (_consecutiveFailures < UnauthorizedFailureThreshold)
                    {
                        return;
                    }

                    _suspended = true;
                    _suspendedIndefinitely = true;
                    _suspendedUntil = null;

                    throw new DownloadTileException(
                        ex.Message,
                        RS.HttpTileProvider_Error_Unauthorized_Title,
                        string.Format(RS.HttpTileProvider_Error_Unauthorized_Message, Info.Name)
                    );
                }
                case HttpRequestException
                {
                    StatusCode: HttpStatusCode.TooManyRequests
                        or >= HttpStatusCode.InternalServerError
                } ex:
                {
                    _logger.LogWarning(
                        "Server error in the tile provider {ProviderId} with status code {StatusCode}",
                        Info.Id,
                        ex.StatusCode
                    );

                    if (_consecutiveFailures < DefaultFailureThreshold)
                    {
                        return;
                    }

                    _suspended = true;
                    IncreaseCooldown();

                    throw new DownloadTileException(
                        ex.Message,
                        RS.HttpTileProvider_Error_ServerError_Title,
                        string.Format(RS.HttpTileProvider_Error_ServerError_Message, Info.Name)
                    );
                }
                default:
                    _logger.LogError(exception, "Failed to download tile from remote");

                    if (_consecutiveFailures < DefaultFailureThreshold)
                    {
                        return;
                    }

                    _suspended = true;
                    IncreaseCooldown();

                    throw new DownloadTileException(
                        exception.Message,
                        RS.HttpTileProvider_Error_NetworkError_Title,
                        string.Format(RS.HttpTileProvider_Error_NetworkError_Message, Info.Name)
                    );
            }
        }

        return;

        void IncreaseCooldown()
        {
            var multiplier = Math.Pow(2, Math.Min(_cooldownExponent, 10));
            var seconds = InitialCooldown.TotalSeconds * multiplier;
            var cooldownSeconds = Math.Min(seconds, MaxCooldown.TotalSeconds);
            _suspendedUntil = _timeProvider.GetUtcNow().AddSeconds(cooldownSeconds);
            _cooldownExponent++;
        }
    }
}

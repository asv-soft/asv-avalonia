using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

internal sealed class NullAppRestartFeature(ILoggerFactory loggerFactory) : IAppRestartFeature
{
    private readonly ILogger<NullAppRestartFeature> _logger =
        loggerFactory.CreateLogger<NullAppRestartFeature>();

    public void Restart()
    {
        _logger.LogWarning(
            "Application restart was scheduled, but restart feature is not configured."
        );
    }
}

using System.Composition;
using Asv.Cfg;
using Microsoft.Extensions.Logging;
using ZLogger;

namespace Asv.Avalonia;

[Export(typeof(ILayoutService))]
[Shared]
public class LayoutService : ILayoutService
{
    public const string LayoutFolder = "layouts.json";
    private readonly IConfiguration _cfg;
    private readonly ILogger<LayoutService> _logger;

    [ImportingConstructor]
    public LayoutService(IAppPath path, ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<LayoutService>();
        var filePath = Path.Combine(path.UserDataFolder, LayoutFolder);
        _cfg = new JsonOneFileConfiguration(
            filePath,
            true,
            TimeSpan.FromSeconds(1),
            false,
            _logger
        );
    }

    public TPocoType Get<TPocoType>(IRoutable source, Lazy<TPocoType> defaultValue)
        where TPocoType : class, new()
    {
        var key = NavigationId.NormalizeTypeId(source.GetPathToRoot().ToString());
        _logger.ZLogTrace($"Get layout for {key})");
        return _cfg.Get(key, defaultValue);
    }

    public void Set<TPocoType>(IRoutable source, TPocoType value)
        where TPocoType : class, new()
    {
        var key = NavigationId.NormalizeTypeId(source.GetPathToRoot().ToString());
        _logger.ZLogTrace($"Set layout for {key})");
        _cfg.Set(key, value);
    }
}

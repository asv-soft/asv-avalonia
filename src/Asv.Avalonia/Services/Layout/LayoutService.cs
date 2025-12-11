using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Composition;
using Asv.Cfg;
using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia;

[Export(typeof(ILayoutService))]
[Shared]
public class LayoutService : AsyncDisposableOnce, ILayoutService
{
    private const string ViewIdPart = "_view";
    private readonly JsonOneFileConfiguration _cfg;
    private readonly InMemoryConfiguration _cfgInMemory;
    private readonly ILogger<LayoutService> _logger;
    private readonly ConcurrentDictionary<string, object?> _cache = new();
    private readonly Lock _lock = new();

    public const string LayoutFolder = "layouts.json";

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
        _cfgInMemory = new InMemoryConfiguration(_logger);

        _sub1 = _cfgInMemory
            .OnChanged.Synchronize()
            .Subscribe(kvp =>
            {
                if (_cache.TryAdd(kvp.Key, kvp.Value))
                {
                    _logger.ZLogTrace(
                        $"Configuration was cached for id = {kvp.Key}, type = {kvp.Value?.GetType()}"
                    );
                    return;
                }

                if (_cache.ContainsKey(kvp.Key))
                {
                    _cache[kvp.Key] = kvp.Value;
                    _logger.ZLogTrace(
                        $"Cached configuration was updated for id = {kvp.Key}, new type = {kvp.Value?.GetType()}"
                    );
                    return;
                }

                _logger.ZLogWarning(
                    $"Failed to cache configuration for id = {kvp.Key}, type = {kvp.Value?.GetType()}"
                );
            });
    }

    public TPocoType Get<TPocoType>(IRoutable source, Lazy<TPocoType> defaultValue)
        where TPocoType : class, new()
    {
        var key = GetKey(source);
        return Get(key, defaultValue);
    }

    public void SetInMemory<TPocoType>(IRoutable source, TPocoType value)
        where TPocoType : class, new()
    {
        var key = GetKey(source);
        SetInMemory(key, value);
    }

    public void RemoveFromMemory(IRoutable source)
    {
        var key = GetKey(source);
        RemoveFromMemory(key);
    }

    public void FlushFromMemory(IRoutable target)
    {
        ArgumentNullException.ThrowIfNull(target);
        var key = GetKey(target);
        FlushFromMemory(key);
    }

    public void FlushFromMemory(IReadOnlyCollection<IRoutable>? ignoreCollection = null)
    {
        using (_lock.EnterScope())
        {
            var keysToIgnore = ignoreCollection?.Select(GetKey).ToImmutableArray();
            _logger.LogInformation("Started flushing the layout");
            foreach (var kvp in _cache)
            {
                if (keysToIgnore?.Contains(kvp.Key) == true)
                {
                    _logger.LogInformation(
                        "Configuration flush was ignored for id = {Key}",
                        kvp.Key
                    );
                    continue;
                }

                _cfg.Set(kvp.Key, kvp.Value);
                _logger.ZLogTrace(
                    $"Configuration was flushed for id = {kvp.Key}, type = {kvp.Value?.GetType()}"
                );
            }

            _logger.LogInformation("Finished flushing the layout");
        }
    }

    #region View Logic

    public TPocoType Get<TPocoType>(StyledElement source, Lazy<TPocoType> defaultValue)
        where TPocoType : class, new()
    {
        var key = GetKey(source);
        return Get(key, defaultValue);
    }

    public void SetInMemory<TPocoType>(StyledElement source, TPocoType value)
        where TPocoType : class, new()
    {
        var key = GetKey(source);
        SetInMemory(key, value);
    }

    public void RemoveFromMemory(StyledElement source)
    {
        var key = GetKey(source);
        RemoveFromMemory(key);
    }

    public void FlushFromMemory(StyledElement source)
    {
        var key = GetKey(source);
        FlushFromMemory(key);
    }

    #endregion

    #region Combined logic

    public void RemoveFromMemoryViewModelAndView(IRoutable source)
    {
        var keyForView = CreateViewKeyFromViewmodel(source);
        RemoveFromMemory(source);
        RemoveFromMemory(keyForView);
    }

    public void RemoveFromMemoryViewModelAndView(StyledElement source)
    {
        var srcRoutable = FindRoutableDataContext(source);
        RemoveFromMemory(source);
        RemoveFromMemory(srcRoutable);
    }

    public void FlushFromMemoryViewModelAndView(IRoutable target)
    {
        var keyForView = CreateViewKeyFromViewmodel(target);
        FlushFromMemory(target);
        FlushFromMemory(keyForView);
    }

    public void FlushFromMemoryViewModelAndView(StyledElement source)
    {
        var srcRoutable = FindRoutableDataContext(source);
        FlushFromMemory(source);
        FlushFromMemory(srcRoutable);
    }

    #endregion

    private string GetKey(IRoutable source)
    {
        return NavigationId.NormalizeTypeId(source.GetPathToRoot().ToString());
    }

    private TPocoType Get<TPocoType>(string key, Lazy<TPocoType> defaultValue)
        where TPocoType : class, new()
    {
        using (_lock.EnterScope())
        {
            if (_cfgInMemory.Exist(key))
            {
                _logger.ZLogTrace($"Get layout for {key} from memory");
                return _cfgInMemory.Get(key, defaultValue);
            }

            _logger.ZLogTrace($"Get layout for {key} from file");
            return _cfg.Get(key, defaultValue);
        }
    }

    private void SetInMemory<TPocoType>(string key, TPocoType value)
        where TPocoType : class, new()
    {
        using (_lock.EnterScope())
        {
            _logger.ZLogTrace($"Set layout for {key} in memory");
            _cfgInMemory.Set(key, value);
        }
    }

    private void RemoveFromMemory(string key)
    {
        using (_lock.EnterScope())
        {
            try
            {
                _cfgInMemory.Remove(key);
                if (!_cache.TryRemove(key, out _))
                {
                    _logger.ZLogTrace($"Failed to remove layout config for id = {key} from cache");
                    return;
                }
            }
            catch (Exception e)
            {
                _logger.ZLogError(
                    $"Failed to remove layout config for id = {key} from {nameof(InMemoryConfiguration)}",
                    e
                );
            }

            _logger.LogInformation("Removed layout config for id = {Key}", key);
        }
    }

    private void FlushFromMemory(string key)
    {
        using (_lock.EnterScope())
        {
            _logger.ZLogTrace($"Attempt to flush configuration for id = {key}");
            if (_cache.TryGetValue(key, out var value))
            {
                _cfg.Set(key, value);
                _logger.ZLogTrace(
                    $"Configuration was flushed for id = {key}, type = {value?.GetType()}"
                );

                return;
            }

            _logger.ZLogWarning(
                $"Failed to flush configuration for id = {key}, type = {value?.GetType()}"
            );
        }
    }

    private IRoutable FindRoutableDataContext(StyledElement source)
    {
        var control = source;
        while (control is not null)
        {
            if (control.DataContext is IRoutable routable)
            {
                return routable;
            }

            control = control.GetLogicalParent() as Control;
        }

        throw new InvalidOperationException(
            $"No {nameof(IRoutable)} DataContext found in the logical tree of the provided StyledElement."
        );
    }

    private string GetKey(StyledElement source)
    {
        return CreateViewKeyFromViewmodel(FindRoutableDataContext(source));
    }

    private string CreateViewKeyFromViewmodel(IRoutable source)
    {
        return GetKey(source) + ViewIdPart;
    }

    #region Dispose

    private readonly IDisposable _sub1;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cache.Clear();
            _sub1.Dispose();
            _cfg.Dispose();
            _cfgInMemory.Dispose();
        }

        base.Dispose(disposing);
    }

    #endregion

    public IExportInfo Source => SystemModule.Instance;
}

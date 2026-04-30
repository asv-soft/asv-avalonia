using Asv.Cfg;
using Asv.Common;
using Avalonia.Input;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public class HotKeyServiceConfig
{
    public Dictionary<string, string> HotKeys { get; set; } = new();
}

public class HotKeyService : AsyncDisposableOnceBag, IHotKeyService
{
    private readonly IShellHost _host;
    private readonly INavigationService _nav;
    private readonly IConfiguration _cfg;
    private readonly ILogger<HotKeyService> _logger;
    private readonly IReadOnlyDictionary<string, IHotKeyAction> _actions;
    private readonly Dictionary<string, KeyGesture> _currentHotKeys;
    private readonly Subject<KeyGesture> _onHotKey;

    public HotKeyService(
        IShellHost host,
        INavigationService nav,
        IConfiguration cfg,
        IEnumerable<IHotKeyAction> actions,
        ILoggerFactory loggerFactory
    )
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(nav);
        ArgumentNullException.ThrowIfNull(cfg);
        ArgumentNullException.ThrowIfNull(actions);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _host = host;
        _nav = nav;
        _cfg = cfg;
        _logger = loggerFactory.CreateLogger<HotKeyService>();
        _actions = actions
            .GroupBy(x => x.Id)
            .ToDictionary(x => x.Key, x => x.Last());
        _currentHotKeys = LoadHotKeys();
        _onHotKey = new Subject<KeyGesture>().AddTo(ref DisposableBag);
        IsHotKeyEnabled = true;

        host.OnShellLoaded.Subscribe(TryEnableHotKeys).AddTo(ref DisposableBag);
    }

    private Dictionary<string, KeyGesture> LoadHotKeys()
    {
        var config = _cfg.Get<HotKeyServiceConfig>();
        var result = _actions.ToDictionary(x => x.Key, x => x.Value.DefaultHotKey);
        var configChanged = false;

        foreach (var (actionId, hotKeyRaw) in config.HotKeys.ToArray())
        {
            if (!_actions.TryGetValue(actionId, out var action))
            {
                config.HotKeys.Remove(actionId);
                configChanged = true;
                _logger.ZLogWarning($"Hot key action '{actionId}' not found => remove it from config");
                continue;
            }

            if (string.IsNullOrWhiteSpace(hotKeyRaw))
            {
                config.HotKeys.Remove(actionId);
                configChanged = true;
                continue;
            }

            KeyGesture hotKey;
            try
            {
                hotKey = KeyGesture.Parse(hotKeyRaw);
            }
            catch (Exception e)
            {
                config.HotKeys.Remove(actionId);
                configChanged = true;
                _logger.ZLogWarning(
                    e,
                    $"Invalid hot key '{hotKeyRaw}' for action '{actionId}' => remove it from config"
                );
                continue;
            }

            if (hotKey.Equals(action.DefaultHotKey))
            {
                config.HotKeys.Remove(actionId);
                configChanged = true;
                continue;
            }

            result[actionId] = hotKey;
        }

        if (configChanged)
        {
            _cfg.Set(config);
        }

        return result;
    }

    private void TryEnableHotKeys(IShell shell)
    {
        if (_host.TopLevel is null)
        {
            _logger.ZLogWarning($"Cannot enable hot keys: shell host top level is null");
            return;
        }

        _host.TopLevel.KeyDown += OnKeyDown;
        Disposable
            .Create(() => _host.TopLevel.KeyDown -= OnKeyDown)
            .AddTo(ref DisposableBag);
    }

    private async void OnKeyDown(object? sender, KeyEventArgs e)
    {
        try
        {
            if (e.Handled || !IsHotKeyEnabled || e.Key == Key.None)
            {
                return;
            }

            var hotKey = new KeyGesture(e.Key, e.KeyModifiers);
            _onHotKey.OnNext(hotKey);

            var context = _nav.SelectedControl.CurrentValue;
            if (context is null)
            {
                return;
            }

            foreach (var action in _actions.Values)
            {
                if (!_currentHotKeys.TryGetValue(action.Id, out var actionHotKey))
                {
                    continue;
                }

                if (!actionHotKey.Equals(hotKey))
                {
                    continue;
                }

                if (await action.TryExecute(context))
                {
                    e.Handled = true;
                    return;
                }
            }
        }
        catch (Exception exception)
        {
            _logger.ZLogError(
                exception,
                $"Error on hot key [{e.KeyModifiers:F} + {e.Key:G}] handler: {exception.Message}"
            );
        }
    }

    public Observable<KeyGesture> OnHotKey => _onHotKey;

    public bool IsHotKeyEnabled { get; set; }

    public KeyGesture? this[string hotKeyId]
    {
        get
        {
            if (_currentHotKeys.TryGetValue(hotKeyId, out var hotKey))
            {
                return hotKey;
            }

            throw new KeyNotFoundException($"Hot key action '{hotKeyId}' not found.");
        }
        set
        {
            if (!_actions.TryGetValue(hotKeyId, out var action))
            {
                throw new KeyNotFoundException($"Hot key action '{hotKeyId}' not found.");
            }

            var config = _cfg.Get<HotKeyServiceConfig>();
            var effectiveHotKey = value ?? action.DefaultHotKey;
            _currentHotKeys[hotKeyId] = effectiveHotKey;

            if (effectiveHotKey.Equals(action.DefaultHotKey))
            {
                config.HotKeys.Remove(hotKeyId);
            }
            else
            {
                config.HotKeys[hotKeyId] = effectiveHotKey.ToString();
            }

            _cfg.Set(config);
        }
    }

    public IEnumerable<IHotKeyInfo> Actions => _actions.Values;
}

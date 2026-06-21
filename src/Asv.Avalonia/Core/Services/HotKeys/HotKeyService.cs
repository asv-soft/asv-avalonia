using Asv.Cfg;
using Asv.Common;
using Avalonia.Controls;
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
    private readonly IConfiguration _cfg;
    private readonly ILogger<HotKeyService> _logger;
    private readonly IReadOnlyDictionary<string, IHotKeyAction> _actions;
    private readonly Dictionary<string, KeyGesture> _currentHotKeys;
    private readonly IReadOnlyDictionary<string, BindableReactiveProperty<bool>> _canExecute;
    private readonly Subject<KeyGesture> _onHotKey;
    private readonly Subject<(IHotKeyInfo Action, KeyGesture Gesture)> _onHotKeyGestureChanged;

    public HotKeyService(
        IShellHost host,
        IConfiguration cfg,
        IEnumerable<IHotKeyAction> actions,
        ILoggerFactory loggerFactory
    )
    {
        ArgumentNullException.ThrowIfNull(host);
        ArgumentNullException.ThrowIfNull(cfg);
        ArgumentNullException.ThrowIfNull(actions);
        ArgumentNullException.ThrowIfNull(loggerFactory);

        _host = host;
        _cfg = cfg;
        _logger = loggerFactory.CreateLogger<HotKeyService>();
        _actions = actions.GroupBy(x => x.ActionId).ToDictionary(x => x.Key, x => x.Last());
        _currentHotKeys = LoadHotKeys();
        _canExecute = _actions.ToDictionary(
            x => x.Key,
            _ => new BindableReactiveProperty<bool>().AddTo(ref DisposableBag)
        );
        _onHotKey = new Subject<KeyGesture>().AddTo(ref DisposableBag);
        _onHotKeyGestureChanged = new Subject<(IHotKeyInfo Action, KeyGesture Gesture)>().AddTo(
            ref DisposableBag
        );
        IsHotKeyEnabled = true;

        host.ExecuteNowOrWhenShellLoaded(TryEnableHotKeys).AddTo(ref DisposableBag);
        host.ExecuteNowOrWhenShellLoaded(TrackCanExecute).AddTo(ref DisposableBag);
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
                _logger.ZLogWarning(
                    $"Hot key action '{actionId}' not found => remove it from config"
                );
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
        var topLevel = TopLevelHelper.GetTopLevel();
        if (topLevel is null)
        {
            _logger.ZLogWarning($"Cannot enable hot keys: no top level available");
            return;
        }

        topLevel.KeyDown += OnKeyDown;
        Disposable.Create(() => topLevel.KeyDown -= OnKeyDown).AddTo(ref DisposableBag);
    }

    private void TrackCanExecute(IShell shell)
    {
        UpdateCanExecute(shell.Navigation.SelectedControl.CurrentValue);
        shell.Navigation.SelectedControl.Subscribe(UpdateCanExecute).AddTo(ref DisposableBag);
    }

    private void UpdateCanExecute(IViewModel? context)
    {
        var actualContext = context ?? _host.Shell;
        foreach (var (actionId, action) in _actions)
        {
            _canExecute[actionId].Value =
                actualContext is not null && action.CanExecute(actualContext);
        }
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

            var context = _host.Shell?.Navigation.SelectedControl.CurrentValue;
            if (context is null)
            {
                return;
            }

            foreach (var action in _actions.Values)
            {
                if (!_currentHotKeys.TryGetValue(action.ActionId, out var actionHotKey))
                {
                    continue;
                }

                if (!actionHotKey.Equals(hotKey))
                {
                    continue;
                }

                await action.Execute(context);
                e.Handled = true;
                return;
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

    public Observable<(IHotKeyInfo Action, KeyGesture Gesture)> OnHotKeyGestureChanged =>
        _onHotKeyGestureChanged;

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
            var changed =
                !_currentHotKeys.TryGetValue(hotKeyId, out var currentHotKey)
                || !currentHotKey.Equals(effectiveHotKey);
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
            if (changed)
            {
                _onHotKeyGestureChanged.OnNext((action, effectiveHotKey));
            }
        }
    }

    public IEnumerable<IHotKeyInfo> Actions => _actions.Values;

    public Observable<bool> ObserveCanExecute(string actionId)
    {
        if (_canExecute.TryGetValue(actionId, out var canExecute))
        {
            return canExecute;
        }

        throw new KeyNotFoundException($"Hot key action '{actionId}' not found.");
    }
}

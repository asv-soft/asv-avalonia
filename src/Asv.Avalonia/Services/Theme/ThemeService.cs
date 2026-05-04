using System.Collections.Immutable;
using Asv.Cfg;
using Asv.Common;
using Avalonia;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public class ThemeServiceConfig
{
    public string Theme { get; set; } = ThemeService.DarkTheme;
    public bool IsCompact { get; set; } = false;
}

public class ThemeService : AsyncDisposableOnce, IThemeService
{
    public const string DarkTheme = "dark";
    public const string LightTheme = "light";
    public const string DefaultTheme = "default";
    private readonly IConfiguration _cfgSvc;
    private readonly ImmutableArray<ThemeItem> _themes;
    private readonly ThemeServiceConfig _config;
    private readonly IDisposable _sub1;
    private readonly ILogger<ThemeService> _logger;
    private readonly IDisposable _sub2;

    public ThemeService(IConfiguration cfgSvc, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(cfgSvc);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _cfgSvc = cfgSvc;
        _logger = loggerFactory.CreateLogger<ThemeService>();
        _themes =
        [
            new ThemeItem(DarkTheme, RS.ThemeService_Dark, ThemeVariant.Dark),
            new ThemeItem(LightTheme, RS.ThemeService_Light, ThemeVariant.Light),
        ];
        _config = cfgSvc.Get<ThemeServiceConfig>();

        var theme = _themes.FirstOrDefault(x => x.Id == _config.Theme) ?? _themes[0];
        CurrentTheme = new SynchronizedReactiveProperty<IThemeInfo>(theme);
        _sub1 = CurrentTheme.Subscribe(SetTheme);

        IsCompact = new SynchronizedReactiveProperty<bool>(_config.IsCompact);
        _sub2 = IsCompact.Subscribe(SaveCompact);
    }

    private void SaveCompact(bool isCompactMode)
    {
        var fluentTheme = Application
            .Current?.Styles.SelectMany(x => x.Children)
            .FirstOrDefault(x => x is FluentTheme);
        if (fluentTheme is FluentTheme theme)
        {
            if (_config.IsCompact != isCompactMode)
            {
                _config.IsCompact = isCompactMode;
                theme.DensityStyle = isCompactMode ? DensityStyle.Compact : DensityStyle.Normal;
                _cfgSvc.Set(_config);
            }
        }
    }

    private void SetTheme(IThemeInfo info)
    {
        if (info is not ThemeItem item)
        {
            _logger.ZLogError($"Invalid theme item");
            return;
        }

        if (Application.Current != null)
        {
            Application.Current.RequestedThemeVariant = item.Theme;
        }

        if (_config.Theme != info.Id)
        {
            _config.Theme = info.Id;
            _cfgSvc.Set(_config);
        }
    }

    public IEnumerable<IThemeInfo> Themes => _themes;
    public SynchronizedReactiveProperty<IThemeInfo> CurrentTheme { get; }
    public SynchronizedReactiveProperty<bool> IsCompact { get; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sub1.Dispose();
            _sub2.Dispose();
            CurrentTheme.Dispose();
            IsCompact.Dispose();
        }

        base.Dispose(disposing);
    }
}

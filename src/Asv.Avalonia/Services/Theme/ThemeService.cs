using System.Collections.Immutable;
using System.Composition;
using Asv.Cfg;
using Asv.Common;
using Avalonia;
using Avalonia.Styling;
using Microsoft.Extensions.Logging;
using R3;
using ZLogger;

namespace Asv.Avalonia;

public class ThemeServiceConfig
{
    public string Theme { get; set; } = ThemeService.DarkTheme;
}

[Export(typeof(IThemeService))]
[Shared]
public class ThemeService : AsyncDisposableOnce, IThemeService
{
    public const string DarkTheme = "dark";
    public const string LightTheme = "light";
    private readonly IConfiguration _cfgSvc;
    private readonly ImmutableArray<ThemeItem> _themes;
    private readonly ThemeServiceConfig _config;
    private readonly IDisposable _sub1;
    private readonly ILogger<ThemeService> _logger;

    [ImportingConstructor]
    public ThemeService(IConfiguration cfgSvc, ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(cfgSvc);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        _cfgSvc = cfgSvc;
        _logger = loggerFactory.CreateLogger<ThemeService>();
        _themes =
        [
            new ThemeItem(DarkTheme, "Dark", ThemeVariant.Dark),
            new ThemeItem(LightTheme, "Light", ThemeVariant.Light),
        ];
        _config = cfgSvc.Get<ThemeServiceConfig>();
        var theme = _themes.FirstOrDefault(x => x.Id == _config.Theme) ?? _themes[0];
        CurrentTheme = new BindableReactiveProperty<IThemeInfo>(theme);
        _sub1 = CurrentTheme.Subscribe(SaveTheme);
    }

    private void SaveTheme(IThemeInfo info)
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

        _config.Theme = info.Id;
        _cfgSvc.Set(_config);
    }

    public IEnumerable<IThemeInfo> Themes => _themes;
    public ReactiveProperty<IThemeInfo> CurrentTheme { get; }
}

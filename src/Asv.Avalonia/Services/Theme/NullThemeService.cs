using System.Collections.Immutable;
using Avalonia.Styling;
using R3;

namespace Asv.Avalonia;

public class NullThemeService : IThemeService, IDisposable
{
    public static IThemeService Instance { get; } = new NullThemeService();

    private readonly ImmutableArray<IThemeInfo> _themes =
    [
        new ThemeItem(ThemeService.DarkTheme, RS.ThemeService_Dark, ThemeVariant.Dark),
        new ThemeItem(ThemeService.LightTheme, RS.ThemeService_Light, ThemeVariant.Light),
    ];

    private NullThemeService() { }

    public IEnumerable<IThemeInfo> Themes => _themes;
    public ReactiveProperty<IThemeInfo> CurrentTheme { get; } = new();
    public ReactiveProperty<bool> IsCompact { get; } = new();

    public void Dispose()
    {
        CurrentTheme.Dispose();
    }
}

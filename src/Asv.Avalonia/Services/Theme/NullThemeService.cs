using System.Collections.Immutable;
using Avalonia.Styling;
using R3;

namespace Asv.Avalonia;

public class NullThemeService : IThemeService, IDisposable
{
    public static IThemeService Instance { get; } = new NullThemeService();

    private readonly ImmutableArray<IThemeInfo> _themes =
    [
        new ThemeItem(ThemeService.DarkTheme, "Dark", ThemeVariant.Dark),
        new ThemeItem(ThemeService.LightTheme, "Light", ThemeVariant.Light),
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

using Asv.Common;
using Asv.Modeling;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public class ThemeProperty : PropertyComboBoxViewModel
{
    public const string ViewModelId = "theme";

    private readonly IThemeService _svc;
    private readonly IUndoChangeSink<ValueUndoChange<string>> _undoHandler;

    public ThemeProperty()
        : this(DesignTime.ThemeService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    public ThemeProperty(IThemeService svc)
        : base(ViewModelId, false)
    {
        _svc = svc;
        Header = RS.SettingsAppearanceView_AppTheme_Title;
        Description = RS.ChangeThemeCommand_CommandInfo_Description;
        Icon = MaterialIconKind.ThemeLightDark;
        IconColor = AsvColorKind.Info5;

        foreach (var theme in svc.Themes)
        {
            ItemsSource.Add(new ThemeOptionViewModel(theme));
        }

        _undoHandler = Undo.RegisterValue<string>("default", ApplyTheme, ApplyTheme)
            .DisposeItWith(Disposable);

        svc.CurrentTheme.Skip(1)
            .ObserveOnUIThreadDispatcher()
            .Subscribe(value => OnChangeByModel(value))
            .DisposeItWith(Disposable);
        OnChangeByModel(svc.CurrentTheme.Value);
    }

    protected override ValueTask ApplyFromUser(IHeadlinedViewModel item, CancellationToken cancel)
    {
        if (item is not ThemeOptionViewModel option)
        {
            return ValueTask.CompletedTask;
        }

        var oldValue = _svc.CurrentTheme.Value?.Id;
        if (oldValue == option.Theme.Id)
        {
            return ValueTask.CompletedTask;
        }

        ApplyTheme(option.Theme.Id);

        if (oldValue is not null)
        {
            _undoHandler.PublishUpdate(oldValue, option.Theme.Id);
        }

        return ValueTask.CompletedTask;
    }

    private void ApplyTheme(string themeId)
    {
        var theme = _svc.Themes.FirstOrDefault(x => x.Id == themeId);
        if (theme is null)
        {
            return;
        }

        _svc.CurrentTheme.Value = theme;
    }

    private void OnChangeByModel(IThemeInfo? modelValue)
    {
        ApplyValueFromModel(FindTheme(modelValue?.Id));
    }

    private ThemeOptionViewModel? FindTheme(string? themeId)
    {
        return ItemsSource.OfType<ThemeOptionViewModel>().FirstOrDefault(x => x.Theme.Id == themeId)
            ?? ItemsSource.OfType<ThemeOptionViewModel>().FirstOrDefault();
    }

    private sealed class ThemeOptionViewModel : HeadlinedViewModel
    {
        public ThemeOptionViewModel(IThemeInfo theme)
            : base(theme.Id)
        {
            Theme = theme;
            Header = theme.Name;
            Icon = GetIcon(theme.Id);
            IconColor = GetIconColor(theme.Id);
        }

        public IThemeInfo Theme { get; }

        private static MaterialIconKind GetIcon(string themeId)
        {
            return themeId switch
            {
                ThemeService.LightTheme => MaterialIconKind.WhiteBalanceSunny,
                ThemeService.DarkTheme => MaterialIconKind.WeatherNight,
                _ => MaterialIconKind.ThemeLightDark,
            };
        }

        private static AsvColorKind GetIconColor(string themeId)
        {
            return themeId switch
            {
                ThemeService.LightTheme => AsvColorKind.Info4,
                ThemeService.DarkTheme => AsvColorKind.Info8,
                _ => AsvColorKind.Info5,
            };
        }
    }
}

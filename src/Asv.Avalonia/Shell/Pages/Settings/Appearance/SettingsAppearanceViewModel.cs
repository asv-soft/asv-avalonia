using System.Composition;
using R3;

namespace Asv.Avalonia;

[ExportSettings(PageId)]
public class SettingsAppearanceViewModel : RoutableViewModel, ISettingsSubPage
{
    public const string PageId = "appearance";

    #region DesignTime

    public SettingsAppearanceViewModel()
        : this(DesignTime.ThemeService)
    {
        DesignTime.ThrowIfNotDesignMode();
    }

    #endregion

    [ImportingConstructor]
    public SettingsAppearanceViewModel(IThemeService themeService)
        : base(PageId)
    {
        Theme = new ThemeProperty(themeService) { Parent = this };
    }

    public ThemeProperty Theme { get; }

    public ValueTask Init(ISettingsPage context)
    {
        return ValueTask.CompletedTask;
    }

    public override ValueTask<IRoutable> Navigate(string id)
    {
        if (id == Theme.Id)
        {
            return ValueTask.FromResult<IRoutable>(Theme);
        }

        return ValueTask.FromResult<IRoutable>(this);
    }
}

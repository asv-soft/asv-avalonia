using Avalonia.Controls;

namespace Asv.Avalonia;

public static class DesignTime
{
    public const string Id = "DesingTimeId";

    public static void ThrowIfNotDesignMode()
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This method is for design mode only");
        }
    }

    public static ILogService Log => NullLogService.Instance;
    public static IShellHost ShellHost => NullShellHost.Instance;
    public static INavigationService Navigation => NullNavigationService.Instance;
    public static IUnitService UnitService => NullUnitService.Instance;
    public static IContainerHost ContainerHost => NullContainerHost.Instance;
    public static IThemeService ThemeService => NullThemeService.Instance;
    public static ILocalizationService LocalizationService => NullLocalizationService.Instance;
    public static ILogService LogService => NullLogService.Instance;
    public static ICommandService CommandService => NullCommandService.Instance;
}

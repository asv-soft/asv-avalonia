using Asv.Cfg;
using Avalonia.Controls;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Asv.Avalonia;

public static class DesignTime
{
    public static NavigationId Id => NavigationId.GenerateRandom();

    public static void ThrowIfNotDesignMode()
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This method is for design mode only");
        }
    }

    public static IConfiguration Configuration { get; } = new InMemoryConfiguration();
    public static ILoggerFactory LoggerFactory => NullLoggerFactory.Instance;
    public static IShellHost ShellHost => NullShellHost.Instance;
    public static INavigationService Navigation => NullNavigationService.Instance;
    public static IUnitService UnitService => NullUnitService.Instance;
    public static IContainerHost ContainerHost => NullContainerHost.Instance;
    public static IThemeService ThemeService => NullThemeService.Instance;
    public static ILocalizationService LocalizationService => NullLocalizationService.Instance;
    public static ILogReaderService LogReaderService => NullLogReaderService.Instance;
    public static ICommandService CommandService => NullCommandService.Instance;
}

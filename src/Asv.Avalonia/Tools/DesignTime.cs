using System.Windows.Input;
using Asv.Cfg;
using Avalonia.Controls;
using Material.Icons;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using R3;

namespace Asv.Avalonia;

public static class DesignTime
{
    public static NavigationId Id => NavigationId.GenerateRandom();
    public static ICommand EmptyCommand = new ReactiveCommand();

    public static void ThrowIfNotDesignMode()
    {
        if (!Design.IsDesignMode)
        {
            throw new InvalidOperationException("This method is for design mode only");
        }
    }

    public static MaterialIconKind RandomImage =>
        Enum.GetValues(typeof(MaterialIconKind))
            .Cast<MaterialIconKind>()
            .Skip(Random.Shared.Next(1, Enum.GetValues<MaterialIconKind>().Length))
            .First();
    public static IAppStartupService AppStartupService => NullAppStartupService.Instance;
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
    public static IFileAssociationService FileAssociation => NullFileAssociationService.Instance;
}

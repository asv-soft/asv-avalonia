using Avalonia.Controls;

namespace Asv.Avalonia;

public static class DesignTime
{
    public static void ThrowIfNotDesignMode()
    {
        if (Design.IsDesignMode == false)
        {
            throw new InvalidOperationException("This method is for design mode only");
        }
    }

    public static IAppHost AppHost => NullAppHost.Instance;

    public static IThemeService ThemeService => NullThemeService.Instance;
    public static ICommandService CommandService => NullCommandService.Instance;
}

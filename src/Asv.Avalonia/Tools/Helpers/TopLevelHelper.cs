using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace Asv.Avalonia;

public static class TopLevelHelper
{
    /// <summary>
    /// Gets the <see cref="TopLevel"/> the application is currently presenting on, or <c>null</c>
    /// if it cannot be resolved (no application lifetime, e.g. design-time or headless).
    /// </summary>
    /// <returns>The current <see cref="TopLevel"/> or <c>null</c>.</returns>
    public static TopLevel? GetTopLevel()
    {
        switch (Application.Current?.ApplicationLifetime)
        {
            case IClassicDesktopStyleApplicationLifetime desktop:
                foreach (var window in desktop.Windows)
                {
                    if (window.IsActive)
                    {
                        return window;
                    }
                }

                return desktop.MainWindow;

            case ISingleViewApplicationLifetime singleView:
                return TopLevel.GetTopLevel(singleView.MainView);

            default:
                return null;
        }
    }
}

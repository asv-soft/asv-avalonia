using Avalonia.Controls;

namespace Asv.Avalonia.Launcher;

public class LauncherWindow : Window
{
    public LauncherWindow()
    {
        Width = 560;
        Height = 280;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowDecorations = WindowDecorations.None;
        Topmost = true;
        Title = "Launcher";
    }
}

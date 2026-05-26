using Avalonia.Controls;

namespace Asv.Avalonia.Launcher;

public class LauncherWindow : Window
{
    public LauncherWindow()
    {
        Width = 420;
        Height = 420;
        CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        WindowDecorations = WindowDecorations.None;
        Topmost = true;
        Title = "Launcher";
    }
}

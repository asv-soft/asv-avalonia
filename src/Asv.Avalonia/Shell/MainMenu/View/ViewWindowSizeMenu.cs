using Asv.Common;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

public class ViewWindowSizeMenu : MenuItem
{
    public const string MenuId = $"{ViewMenu.MenuId}-window-size-1920x1080";

    public ViewWindowSizeMenu()
        : this(1920, 1080, 30)
    {
    }

    protected ViewWindowSizeMenu(int width, int height, int order)
        : base($"{ViewMenu.MenuId}-window-size-{width}x{height}", $"{width}×{height}", ViewMenu.MenuId)
    {
        Icon = MaterialIconKind.Resize;
        Order = order;
        Command = new ReactiveCommand(_ => ResizeMainWindow(width, height)).DisposeItWith(Disposable);
    }

    private static void ResizeMainWindow(int width, int height)
    {
        if (
            Application.Current?.ApplicationLifetime
                is not IClassicDesktopStyleApplicationLifetime lifetime
            || lifetime.MainWindow is not { } window
        )
        {
            return;
        }

        window.WindowState = WindowState.Normal;
        window.Width = width;
        window.Height = height;
    }
}

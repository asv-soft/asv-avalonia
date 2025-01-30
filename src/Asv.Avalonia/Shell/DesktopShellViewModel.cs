using Avalonia.Controls.ApplicationLifetimes;

namespace Asv.Avalonia;

public class DesktopShellViewModel : ShellViewModel
{
    public DesktopShellViewModel(
        IClassicDesktopStyleApplicationLifetime lifetime,
        IContainerHost containerHost
    )
        : base(containerHost)
    {
        lifetime.MainWindow = new ShellWindow { DataContext = this };
    }

    protected override void InternalAddPageToMainTab(IPage export)
    {
        throw new NotImplementedException();
    }
}

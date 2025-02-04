using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace Asv.Avalonia;

public class DesktopShellViewModel : ShellViewModel
{
    public DesktopShellViewModel(
        IClassicDesktopStyleApplicationLifetime lifetime,
        IContainerHost ioc
    )
        : base(ioc)
    {
        lifetime.MainWindow = new ShellWindow { DataContext = this };
        lifetime.MainWindow.Show();
    }

    protected override ValueTask CloseAsync(CancellationToken cancellationToken)
    {
        if (
            Application.Current != null
            && Application.Current.ApplicationLifetime
                is IClassicDesktopStyleApplicationLifetime lifetime
        )
        {
            lifetime.Shutdown();
        }

        return ValueTask.CompletedTask;
    }
}

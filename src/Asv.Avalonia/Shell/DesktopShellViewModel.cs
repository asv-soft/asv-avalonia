using Asv.Cfg;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace Asv.Avalonia;

public class DesktopShellViewModelConfig { }

public class DesktopShellViewModel : ShellViewModel
{
    private readonly IContainerHost _ioc;

    public DesktopShellViewModel(
        IClassicDesktopStyleApplicationLifetime lifetime,
        IContainerHost ioc
    )
        : base(ioc)
    {
        _ioc = ioc;
        var wnd = ioc.GetExport<ShellWindow>();
        wnd.DataContext = this;
        lifetime.MainWindow = wnd;
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

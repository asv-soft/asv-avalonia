using System.Composition;
using Asv.Cfg;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace Asv.Avalonia;

public class DesktopShellViewModelConfig { }

[Export(ShellId, typeof(IShell))]
public class DesktopShellViewModel : ShellViewModel
{
    public const string ShellId = "shell.desktop";

    private readonly IContainerHost _ioc;

    [ImportingConstructor]
    public DesktopShellViewModel(IContainerHost ioc)
        : base(ioc, ShellId)
    {
        _ioc = ioc;
        var wnd = ioc.GetExport<ShellWindow>();
        wnd.DataContext = this;
        if (
            Application.Current?.ApplicationLifetime
            is not IClassicDesktopStyleApplicationLifetime lifetime
        )
        {
            throw new Exception(
                "ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime"
            );
        }

        lifetime.MainWindow = wnd;
        lifetime.MainWindow.Show();
    }

    protected override ValueTask CloseAsync(CancellationToken cancellationToken)
    {
        if (
            Application.Current?.ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime lifetime
        )
        {
            lifetime.Shutdown();
        }

        return ValueTask.CompletedTask;
    }
}

using System.Composition;
using Asv.Cfg;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[Export(ShellId, typeof(IShell))]
public class MobileShellViewModel : ShellViewModel
{
    public const string ShellId = "shell.mobile";

    [ImportingConstructor]
    public MobileShellViewModel(
        IConfiguration cfg,
        IContainerHost containerHost,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory
    )
        : base(containerHost, layoutService, loggerFactory, cfg, ShellId)
    {
        // do nothing
    }
}

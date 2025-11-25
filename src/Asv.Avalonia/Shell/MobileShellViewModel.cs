using System.Composition;
using Asv.Cfg;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[Export(ShellId, typeof(IShell))]
public sealed class MobileShellViewModel : ShellViewModel
{
    public const string ShellId = "shell.mobile";

    [ImportingConstructor]
    public MobileShellViewModel(
        IConfiguration cfg,
        IContainerHost containerHost,
        ILoggerFactory loggerFactory
    )
        : base(ShellId, containerHost, loggerFactory, cfg)
    {
        // do nothing
    }
}

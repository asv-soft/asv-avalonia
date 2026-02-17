using Asv.Cfg;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public sealed class MobileShellViewModel : ShellViewModel
{
    public const string ShellId = "shell.mobile";

    public MobileShellViewModel(
        IServiceProvider containerHost,
        IConfiguration cfg,
        ILoggerFactory loggerFactory,
        IExtensionService ext
    )
        : base(ShellId, containerHost, loggerFactory, cfg, ext)
    {
        // do nothing
    }
}

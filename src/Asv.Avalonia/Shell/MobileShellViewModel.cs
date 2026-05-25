using Asv.Cfg;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public sealed class MobileShellViewModel : ShellViewModel
{
    public MobileShellViewModel(
        IServiceProvider ioc,
        ILoggerFactory loggerFactory,
        IAppPath appPath,
        IThemeService themeService,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(ioc, loggerFactory, appPath, themeService, dialogService, ext)
    {
        // do nothing
    }
}

using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public class SettingsPageViewModel
    : TreePageViewModel<ISettingsPage, ISettingsSubPage>,
        ISettingsPage
{
    public const string PageId = "settings";
    public const MaterialIconKind PageIcon = MaterialIconKind.Settings;
    public const AsvColorKind PageIconColor = AsvColorKind.None;

    public SettingsPageViewModel()
        : this(
            DesignTime.PageContext,
            
            AppHost.Instance.Services,
            NullLayoutService.Instance,
            DesignTime.LoggerFactory,
            DesignTime.DialogService,
            DesignTime.ExtensionService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        Header = RS.SettingsPageViewModel_Title;
        Icon = PageIcon;
    }

    public SettingsPageViewModel(
        IPageContext context,
        ICommandService svc,
        IServiceProvider host,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(PageId, context, svc, host, layoutService, loggerFactory, dialogService, ext)
    {
        Header = RS.SettingsPageViewModel_Title;
        Icon = PageIcon;
        IconColor = PageIconColor;
    }
}

using System.Composition;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

[ExportPage(PageId)]
public class SettingsPageViewModel
    : TreePageViewModel<ISettingsPage, ISettingsSubPage>,
        ISettingsPage
{
    public const string PageId = "settings";
    public const MaterialIconKind PageIcon = MaterialIconKind.Settings;
    public const AsvColorKind PageIconColor = AsvColorKind.None;

    public SettingsPageViewModel()
        : this(
            DesignTime.CommandService,
            NullContainerHost.Instance,
            NullLayoutService.Instance,
            DesignTime.LoggerFactory,
            DesignTime.DialogService,
            DesignTime.ExtensionService
        )
    {
        DesignTime.ThrowIfNotDesignMode();
        Title = RS.SettingsPageViewModel_Title;
        Icon = PageIcon;
    }

    [ImportingConstructor]
    public SettingsPageViewModel(
        ICommandService svc,
        IContainerHost host,
        ILayoutService layoutService,
        ILoggerFactory loggerFactory,
        IDialogService dialogService,
        IExtensionService ext
    )
        : base(PageId, svc, host, layoutService, loggerFactory, dialogService, ext)
    {
        Title = RS.SettingsPageViewModel_Title;
        Icon = PageIcon;
        IconColor = PageIconColor;
    }

    public override IExportInfo Source => SystemModule.Instance;
}

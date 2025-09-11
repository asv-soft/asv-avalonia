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

    public SettingsPageViewModel()
        : this(
            DesignTime.CommandService,
            NullContainerHost.Instance,
            NullLayoutService.Instance,
            DesignTime.LoggerFactory
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
        ILoggerFactory loggerFactory
    )
        : base(PageId, svc, host, layoutService, loggerFactory)
    {
        Title = RS.SettingsPageViewModel_Title;
    }

    public override IExportInfo Source => SystemModule.Instance;
}

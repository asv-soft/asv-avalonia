using System.Composition;
using Asv.Cfg;
using Material.Icons;
using Microsoft.Extensions.Logging;

namespace Asv.Avalonia;

public sealed class SettingsPageViewModelConfig : PageConfig { }

[ExportPage(PageId)]
public class SettingsPageViewModel
    : TreePageViewModel<ISettingsPage, ISettingsSubPage, SettingsPageViewModelConfig>,
        ISettingsPage
{
    public const string PageId = "settings";
    public const MaterialIconKind PageIcon = MaterialIconKind.Settings;

    public SettingsPageViewModel()
        : this(
            DesignTime.CommandService,
            NullContainerHost.Instance,
            DesignTime.Configuration,
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
        IConfiguration configuration,
        ILoggerFactory loggerFactory
    )
        : base(PageId, svc, host, configuration, loggerFactory)
    {
        Title = RS.SettingsPageViewModel_Title;
    }

    public override IExportInfo Source => SystemModule.Instance;
}

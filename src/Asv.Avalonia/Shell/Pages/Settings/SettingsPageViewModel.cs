using System.Collections.Specialized;
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
            DesignTime.LoggerFactory,
            DesignTime.DialogService
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
        ILoggerFactory loggerFactory, 
        IDialogService dialogService
    )
        : base(PageId, svc, host, configuration, loggerFactory, dialogService)
    {
        Title = RS.SettingsPageViewModel_Title;
    }

    protected override void InternalInitArgs(NameValueCollection args)
    {
        base.InternalInitArgs(args);
    }

    public override IExportInfo Source => SystemModule.Instance;
}

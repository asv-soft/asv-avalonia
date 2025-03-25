using System.Composition;
using Material.Icons;
using R3;

namespace Asv.Avalonia;

[ExportPage(PageId)]
public class SettingsPageViewModel
    : TreePageViewModel<ISettingsPage, ISettingsSubPage>,
        ISettingsPage
{
    public const string PageId = "settings";
    public const MaterialIconKind PageIcon = MaterialIconKind.Settings;

    public SettingsPageViewModel()
        : base(PageId, DesignTime.CommandService, NullContainerHost.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        Title.OnNext(RS.SettingsPageViewModel_Title);
        Icon.Value = PageIcon;
    }

    [ImportingConstructor]
    public SettingsPageViewModel(ICommandService svc, IContainerHost host)
        : base(PageId, svc, host)
    {
        Title.OnNext(RS.SettingsPageViewModel_Title);
    }

    public override IExportInfo Source => SystemModule.Instance;
}

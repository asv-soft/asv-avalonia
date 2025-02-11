using System.Composition;
using R3;

namespace Asv.Avalonia;

[ExportViewFor(typeof(SettingsPageViewModel))]
public class SettingsPageView : TreePageView { }

[ExportPage(PageId)]
public class SettingsPageViewModel : TreePageViewModel<ISettingsPage>, ISettingsPage
{
    public const string PageId = "settings";

    public SettingsPageViewModel()
        : base(PageId, DesignTime.CommandService, NullContainerHost.Instance)
    {
        DesignTime.ThrowIfNotDesignMode();
        Title.OnNext(RS.SettingsPageViewModel_Title);
    }

    [ImportingConstructor]
    public SettingsPageViewModel(ICommandService svc, IContainerHost host)
        : base(PageId, svc, host)
    {
        Title.OnNext(RS.SettingsPageViewModel_Title);
    }
}

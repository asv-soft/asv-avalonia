using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia;

public class HomePageSettingsExtension : IExtensionFor<IHomePage>
{
    public const string StaticId = "ext.home.settings";

    string ISupportId<string>.Id => StaticId;

    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        var action = new ActionViewModel("open-settings")
        {
            Header = RS.OpenSettingsCommand_Action_Title,
            Description = RS.OpenSettingsCommand_Action_Description,
            Icon = SettingsPageViewModel.PageIcon,
            Command = new ReactiveCommand(_ =>
                context.GoTo(new NavPath(new NavId(SettingsPageViewModel.PageId)))
            ).DisposeItWith(contextDispose),
        }.DisposeItWith(contextDispose);

        context.Tools.Add(action);
    }
}

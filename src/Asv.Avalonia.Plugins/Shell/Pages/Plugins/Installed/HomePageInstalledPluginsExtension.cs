using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia.Plugins;

public class HomePageInstalledPluginsExtension : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        var action = new ActionViewModel("open.plugins.installed")
        {
            Header = RS.OpenInstalledPluginsCommand_Action_Title,
            Description = RS.OpenInstalledPluginsCommand_Action_Description,
            Icon = InstalledPluginsPageViewModel.PageIcon,
            Command = new ReactiveCommand(_ =>
                context.GoTo(new NavPath(new NavId(InstalledPluginsPageViewModel.PageId)))
            ).DisposeItWith(contextDispose),
        }.DisposeItWith(contextDispose);

        context.Tools.Add(action);
    }
}

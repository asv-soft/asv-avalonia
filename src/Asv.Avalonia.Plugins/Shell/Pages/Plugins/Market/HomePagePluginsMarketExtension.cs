using Asv.Common;
using Asv.Modeling;
using R3;

namespace Asv.Avalonia.Plugins;

public class HomePagePluginsMarketExtension : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        var action = new ActionViewModel("open-plugins-market")
        {
            Header = RS.OpenPluginsMarketCommand_Action_Title,
            Description = RS.OpenPluginsMarketCommand_Action_Description,
            Icon = PluginsMarketPageViewModel.PageIcon,
            Command = new ReactiveCommand(_ =>
                context.GoTo(new NavPath(new NavId(PluginsMarketPageViewModel.PageId)))
            ).DisposeItWith(contextDispose),
        }.DisposeItWith(contextDispose);

        context.Tools.Add(action);
    }
}

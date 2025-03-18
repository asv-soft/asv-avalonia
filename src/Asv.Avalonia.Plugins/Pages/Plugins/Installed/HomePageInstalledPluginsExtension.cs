using Asv.Common;
using R3;

namespace Asv.Avalonia.Plugins;

[ExportExtensionFor<IHomePage>]
public class HomePageInstalledPluginsExtension : IExtensionFor<IHomePage>
{
    public void Extend(IHomePage context, CompositeDisposable contextDispose)
    {
        context.Tools.Add(
            OpenInstalledPluginsCommand.StaticInfo.CreateAction().DisposeItWith(contextDispose)
        );
    }
}
